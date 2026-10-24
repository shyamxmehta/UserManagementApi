using Microsoft.Data.SqlClient;

namespace UserManagementApi.Data.Access
{
    public class DbExecutor : IDbExecutor
    {
        public async Task<List<T>?> QueryAsync<T>(SqlCommand cmd, Func<SqlDataReader, T> map)
        {
            var results = new List<T>();

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                results.Add(map(reader));

            return results;
        }

        public async Task<T?> QuerySingleAsync<T>(SqlCommand cmd, Func<SqlDataReader, T> map)
        {
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return default;

            return map(reader);
        }
    }
}
