using Microsoft.Data.SqlClient;

namespace UserManagementApi.Data.Access
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public ConnectionFactory(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DemoAppDb")
                ?? throw new InvalidOperationException("Connection string not found.");
        }
        public async Task<SqlConnection> CreateOpenSqlConnection(CancellationToken ct = default)
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            return conn;
        }
    }
}
