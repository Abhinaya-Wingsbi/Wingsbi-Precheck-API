using Microsoft.EntityFrameworkCore;
using Godrej.Precheck.Models.DataModel.Archive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Database
{
    /// <summary>
    /// Database context for backup database (PrecheckDB_QA)
    /// Used specifically for archive functionality
    /// </summary>
    public class BackupDbContext : DbContext, IBackupDbContext
    {
        public BackupDbContext(DbContextOptions<BackupDbContext> options) : base(options)
        {
        }

        // DbSets for archive tables
        public DbSet<ArchiveCompData> ArchiveCompData { get; set; }
        public DbSet<DrawingCompMapping> DrawingCompMapping { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure archive tables
            modelBuilder.Entity<ArchiveCompData>().ToTable("tbl_archive_comp_data");
            modelBuilder.Entity<DrawingCompMapping>().ToTable("tbl_drawing_comp_mapping");
            
            // Configure any specific model configurations for backup database
            // if needed in the future
        }
    }

    /// <summary>
    /// Interface for backup database context
    /// </summary>
    public interface IBackupDbContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
    }
}
