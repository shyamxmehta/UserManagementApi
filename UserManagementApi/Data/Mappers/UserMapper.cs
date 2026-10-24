using UserManagementApi.Data.Extentions;
using UserManagementApi.Modules.Users.DTOs;
using UserManagementApi.Modules.Users.Models;
using Microsoft.Data.SqlClient;

namespace UserManagementApi.Data.Mappers
{
    public static class UserMapper
    {
        public static User Map(SqlDataReader reader)
        {
            var idIdx = reader.GetOrdinal("ID");
            var usernameIdx = reader.GetOrdinal("Username");
            var passwordIdx = reader.GetOrdinal("PasswordHash");
            var roleIdx = reader.GetOrdinal("Role");
            var refreshTokenIdx = reader.GetOrdinal("RefreshToken");

            return new User
            {
                ID = reader.GetInt64(idIdx),
                Username = reader.GetString(usernameIdx),
                PasswordHash = reader.GetString(passwordIdx),
                Role = reader.GetString(roleIdx),
                RefreshToken = reader.GetStringOrNull(refreshTokenIdx)
            };
        }

        public static GetUsersResponseDTO MapUsers(SqlDataReader reader)
        {
            var idIdx = reader.GetOrdinal("ID");
            var usernameIdx = reader.GetOrdinal("Username");
            var roleIdx = reader.GetOrdinal("Role");

            return new GetUsersResponseDTO
            {
                ID = reader.GetInt64(idIdx),
                Username = reader.GetString(usernameIdx),
                Role = reader.GetString(roleIdx),
            };
        }

        public static Dictionary<string, object> MapToDictionary(SqlDataReader reader)
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
            }
            return row;
        }
    }
}
