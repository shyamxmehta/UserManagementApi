using Microsoft.Data.SqlClient;

namespace UserManagementApi.Data.Access
{
    public interface IDbExecutor
    {
        Task<T?> QuerySingleAsync<T>(SqlCommand cmd, Func<SqlDataReader, T> map);
        Task<List<T>?> QueryAsync<T>(SqlCommand cmd, Func<SqlDataReader, T> map);
    }
}
