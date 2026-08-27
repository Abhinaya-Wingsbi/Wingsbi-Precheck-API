using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace Godrej.Precheck.Repository.Database
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ApplicationDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public string Database
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(_connectionString);
                return builder.InitialCatalog;
            }
        }

        public string DataSource
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(_connectionString);
                return builder.DataSource;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<IEnumerable<T>> GetAll<T>(string query, object parameters)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    return await connection.QueryAsync<T>(query, parameters, commandTimeout: 300);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing query: {ex.Message}", ex);
            }
        }

        public async Task<T> GetSingle<T>(string query, object parameters)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    return await connection.QueryFirstOrDefaultAsync<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing query: {ex.Message}", ex);
            }
        }

        public async Task<int> Execute(string query, object parameters)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    return await connection.ExecuteAsync(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing command: {ex.Message}", ex);
            }
        }

        public async Task<int> Update(string query, object parameters)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    return await connection.ExecuteAsync(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing update query: {ex.Message}", ex);
            }
        }


        public async Task<int> UpdateAsync(string query, object parameters)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    // Execute the query and retrieve the output value
                    var result = await connection.QueryFirstOrDefaultAsync<int>(query, parameters);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing update query: {ex.Message}", ex);
            }
        }


        public async Task<T> ExecuteScalar<T>(string query, object parameters)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    return await connection.ExecuteScalarAsync<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing scalar query: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<T>> ExecuteStoredProcedure<T>(string procName, object parameters)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    return await connection.QueryAsync<T>(
                        procName,
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing stored procedure: {ex.Message}", ex);
            }
        }

        public async Task<int> ExecuteTransaction(string query, object parameters)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var result = await connection.ExecuteAsync(query, parameters, transaction);
                            transaction.Commit();
                            return result;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing transaction: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string query, object parameters)
        {
            try
            {
                using (var connection = CreateConnection())
                {
                    return await connection.QueryAsync<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing query: {ex.Message}", ex);
            }
        }

        public async Task<IDbConnection> CreateOpenConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<T> ExecuteScalarOnConnection<T>(IDbConnection connection, string query, object parameters)
        {
            try
            {
                return await connection.ExecuteScalarAsync<T>(query, parameters);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error executing scalar query: {ex.Message}", ex);
            }
        }
    }
}
