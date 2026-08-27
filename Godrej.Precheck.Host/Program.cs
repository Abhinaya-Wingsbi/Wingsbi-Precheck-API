using Dapper;
using Godrej.Precheck.Repository.Database;
using Godrej.Precheck.Repository.Repository.ArchiveRepository;
using Godrej.Precheck.Repository.Repository.CommonRepository;
using Godrej.Precheck.Repository.Repository.DrawingNumberRepository;
using Godrej.Precheck.Repository.Repository.IdentifierRepository;
using Godrej.Precheck.Repository.Repository.MaterialRequisitionRepository;
using Godrej.Precheck.Repository.Repository.PrecheckRepository;
using Godrej.Precheck.Repository.Repository.QRCodeRepository;
using Godrej.Precheck.Repository.Repository.SopRepository;
using Godrej.Precheck.Repository.Repository.TestingRepository;
using Godrej.Precheck.Repository.Repository.UserRepository;
using Godrej.Precheck.Service.Cache;
using Godrej.Precheck.Service.Helper;
using Godrej.Precheck.Service.MapperSetup;
using Godrej.Precheck.Service.Service.ArchiveService;
using Godrej.Precheck.Service.Service.AuthService;
using Godrej.Precheck.Service.Service.CommonSevice;
using Godrej.Precheck.Service.Service.DrawingNumberService;
using Godrej.Precheck.Service.Service.IdentifierService;
using Godrej.Precheck.Service.Service.MaterialRequisitionService;
using Godrej.Precheck.Service.Service.PrecheckService;
using Godrej.Precheck.Service.Service.QRCodeService;
using Godrej.Precheck.Service.Service.SopService;
using Godrej.Precheck.Service.Service.TestingService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DinkToPdf;
using DinkToPdf.Contracts;
using Serilog;
using Serilog.Events;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// Initialize Mapster mappings
MappingSetup.Init();

// Determine environment
var isAzure = Environment.GetEnvironmentVariable("HOME") != null;

// Get log base directory
var logBaseDirectory = isAzure
    ? Path.Combine(Environment.GetEnvironmentVariable("HOME")!, "LogFiles", "Application")
    : configuration.GetValue<string>("Logging:LogDirectory") ?? "Logs";

// Read logging enabled flag
var enableLogging = configuration.GetValue<bool>("Logging:EnableLogging");

if (enableLogging)
{
    // Create timestamped folder structure
    var dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
    var hourFolder = DateTime.Now.ToString("HH");
    var fullLogPath = Path.Combine(logBaseDirectory, dateFolder, hourFolder);
    Directory.CreateDirectory(fullLogPath);

    // Configure Serilog
    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error || e.Level == LogEventLevel.Fatal)
            .WriteTo.File(
                Path.Combine(fullLogPath, "ERROR.txt"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Infinite,
                retainedFileCountLimit: null
            )
        )
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(e => e.Level <= LogEventLevel.Information)
            .WriteTo.File(
                Path.Combine(fullLogPath, "INFO.txt"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Infinite,
                retainedFileCountLimit: null
            )
        )
        .CreateLogger();

    builder.Host.UseSerilog();
}

// CORS policy
var MyAllowSpecificOrigins = "AllowAll";
DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

// Backup Database context
builder.Services.AddDbContext<BackupDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BackupConnection")));
builder.Services.AddScoped<IBackupDbContext, BackupDbContext>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// Register Repositories
builder.Services.AddScoped<ICommonRepository, CommonRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQRCodeRepository, QRCodeRepository>();
builder.Services.AddScoped<IIdentifierRepository, IdentifierRepository>();
builder.Services.AddScoped<IPrecheckRepository, PrecheckRepository>();
builder.Services.AddScoped<ISopRepository, SopRepository>();
builder.Services.AddScoped<IDrawingNumberRepository, DrawingNumberRepository>();
builder.Services.AddScoped<IMaterialRequisitionRepository, MaterialRequisitionRepository>();
builder.Services.AddScoped<ITestingRepository, TestingRepository>();
// Register Backup Archive Repository (only archive service needed)
builder.Services.AddScoped<IBackupArchiveRepository, BackupArchiveRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddScoped<IQRCodeService, QRCodeSevice>();
builder.Services.AddScoped<IIdentifierService, IdentifierService>();
builder.Services.AddScoped<IPrecheckService, PrecheckService>();
builder.Services.AddScoped<ISopService, SopService>();
builder.Services.AddScoped<IDrawingNumberService, DrawingNumberService>();
builder.Services.AddScoped<IMaterialRequisitionService, MaterialRequisitionService>();
builder.Services.AddScoped<ITestingService, TestingService>();
builder.Services.AddScoped<IHelperService, HelperService>();
// Register Backup Archive Service (only archive service needed)
builder.Services.AddScoped<IBackupArchiveService, BackupArchiveService>();
// Production Order Import
builder.Services.AddScoped<Godrej.Precheck.Repository.Repository.ProductionOrderRepository.IProductionOrderRepository, Godrej.Precheck.Repository.Repository.ProductionOrderRepository.ProductionOrderRepository>();
builder.Services.AddScoped<Godrej.Precheck.Service.Service.ProductionOrderService.IProductionOrderService, Godrej.Precheck.Service.Service.ProductionOrderService.ProductionOrderService>();
// Cache
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, CacheService>();

// DinkToPdf - Singleton because wkhtmltopdf native library is not thread-safe
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

// Swagger & API
// MaxDepth raised from the default (32) because GetSop's response nests one GetSopResponseDto.Children
// list per BOM level, and a real, deeply-nested assembly can legitimately exceed 32 levels - System.Text.Json
// was throwing "A possible object cycle was detected" partway through serialization once it did, which sent
// a 200 status (headers already flushed) but then cut the response body off mid-stream, making the call
// look like it hung rather than surfacing a clear error.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.MaxDepth = 256;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v9.5", new OpenApiInfo
    {
        Title = "Godrej Precheck API",
        Version = "v9.5"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (enableLogging)
{
    app.UseSerilogRequestLogging();
}


app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v9.5/swagger.json", "Godrej Precheck API v9.5");
    c.DisplayRequestDuration();
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
