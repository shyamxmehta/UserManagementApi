using Microsoft.Data.SqlClient;

namespace UserManagementApi.Data.Access
{
    public interface IConnectionFactory
    {
        Task<SqlConnection> CreateOpenSqlConnection(CancellationToken ct = default);
    }
}
