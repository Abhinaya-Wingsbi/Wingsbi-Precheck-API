using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godrej.Precheck.Repository.Database
{
    public interface IApplicationDbContext
    {
        Task<IEnumerable<T>> GetAll<T>(string query, object parameters);
        Task<T> GetSingle<T>(string query, object parameters);
        Task<int> Execute(string query, object parameters);
        Task<T> ExecuteScalar<T>(string query, object parameters);

        Task<int> Update(string query, object parameters);

        Task<int> UpdateAsync(string query, object parameters);
        Task<IEnumerable<T>> ExecuteStoredProcedure<T>(string procName, object parameters);
        Task<int> ExecuteTransaction(string query, object parameters);
        string Database { get; }
        string DataSource { get; }
        Task<IEnumerable<T>> QueryAsync<T>(string query, object param = null);

        // Lets a caller open one connection and reuse it across many calls (e.g. a tight insert
        // loop), instead of each call opening its own SqlConnection.
        Task<IDbConnection> CreateOpenConnectionAsync();
        Task<T> ExecuteScalarOnConnection<T>(IDbConnection connection, string query, object parameters);
    }
}
