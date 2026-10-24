using UserManagementApi.Data.Access;
using UserManagementApi.Data.Mappers;
using UserManagementApi.Modules.Authentication.DTOs;
using UserManagementApi.Modules.Users.DTOs;
using UserManagementApi.Modules.Users.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace UserManagementApi.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConnectionFactory _connection;
        private readonly IDbExecutor _executor;

        public UserRepository(IConnectionFactory connection, IDbExecutor executor)
        {
            _connection = connection;
            _executor = executor;

        }

        public async Task<int?> RegisterUserAsync(AddUserRequestDTO userRequestDTO)
        {
            using var conn = await _connection.CreateOpenSqlConnection();
            using var cmd = new SqlCommand("AddUser", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Username", userRequestDTO.Username);
            cmd.Parameters.AddWithValue("@PasswordHash", userRequestDTO.PasswordHash);
            cmd.Parameters.AddWithValue("@Role", userRequestDTO.Role);

            try
            {
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                    return -1;
                throw new Exception("Unhandled Database Exception");
            }
        }

        public async Task<User?> GetUserAsync(LoginRequestDTO user)
        {

            const string sql = @"
                            SELECT 
                                [ID],
                                [Username],
                                [PasswordHash],
                                [Role],
                                [RefreshToken],
                                [RefreshTokenExpiry],
                                [CreatedAt]
                            FROM [DemoAppDb].[dbo].[Users]
                            WHERE Username = @Username";

            using var conn = await _connection.CreateOpenSqlConnection();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = user.Username;

            try
            {
                var result = await _executor.QuerySingleAsync(cmd, UserMapper.Map);
                return result;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }

            //var passHash = await cmd.ExecuteScalarAsync();

            //if (passHash?.ToString() != user.Password)
            //    return false;

        }

        public async Task<List<GetUsersResponseDTO>?> GetAllUsersAsync()
        {
            //List<User> users = new List<User>();
            using var conn = await _connection.CreateOpenSqlConnection();
            const string sql = @"SELECT * FROM Users";
            using var cmd = new SqlCommand(sql, conn);

            try
            {
                var result = await _executor.QueryAsync(cmd, UserMapper.MapUsers);
                return result;
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }
   
        public async Task<string[]?> GetRolePermissions(User user)
        {
            string sql = @"SELECT [ID] FROM [DemoAppDb].[dbo].[Roles] WHERE Name=@role";
            using var conn = await _connection.CreateOpenSqlConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@role", SqlDbType.NVarChar).Value = user.Role;

            try
            {
                // ExecuteScalarAsync returns an object, so cast it to the expected type
                var result = await cmd.ExecuteScalarAsync();

                // Check for null (e.g., if a WHERE clause matches nothing)
                if (result != null && result != DBNull.Value)
                {
                    var rightId = (int)result; // Example: assuming an int is returned
                                               // Process rightId

                    sql = @"SELECT TOP (1000) [RoleID]
                              ,[Permission]
                              ,[Checksum]
                          FROM [DemoAppDb].[dbo].[RolePermissions]
                          WHERE RoleID = @rightId";

                    using var cmd2 = new SqlCommand(sql, conn);
                    cmd2.Parameters.Add("@rightId", SqlDbType.Int)?.Value = rightId;

                    try
                    {
                        var result2 = await _executor.QueryAsync(cmd2, UserMapper.MapToDictionary);
                        return result2?.Select(row => row["Permission"].ToString()).ToArray()!;
                    }
                    catch (SqlException ex)
                    {
                        // Handle exception
                        return [ex.Message];
                    }
                }

                return null;
            }
            catch (SqlException ex)
            {
                // Handle exception
                return [ex.Message];
            }
        }
        
        public async Task<bool> GiveDeletePermission()
        {
            const string sql = @"IF NOT EXISTS (SELECT 1 FROM UserPermissions WHERE Username='Shyam' and Permission='DELETE_USER')
                                BEGIN
                                INSERT INTO UserPermissions (Username, Permission)
                                VALUES ('Shyam','DELETE_USER')
                                END";
            using var conn = await _connection.CreateOpenSqlConnection();
            using var cmd = new SqlCommand(sql, conn);

            try
            {
                var result = await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (SqlException ex)
            {
                return false;
            }

        }

        public async Task<string[]?> GetUserPermissions(string username)
        {
            string sql = @"SELECT Permission FROM [DemoAppDb].[dbo].[UserPermissions] WHERE Username=@username";
            using var conn = await _connection.CreateOpenSqlConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;

            try
            {
                var result = await _executor.QueryAsync(cmd, UserMapper.MapToDictionary);
                return result?.Select(row => row["Permission"].ToString()).ToArray()!;
            }
            catch (SqlException ex)
            {
                // Handle exception
                return [ex.Message];
            }
        }
        //public async Task<int?> CheckUserExistAsync(string username)
        //{
        //    const string sql = @"
        //                    SELECT 
        //                        [ID]
        //                    FROM [DemoAppDb].[dbo].[Users]
        //                    WHERE Username = @Username";

        //    using var conn = await _connection.CreateOpenSqlConnection();
        //    using var cmd = new SqlCommand(sql, conn);

        //    cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;

        //    try
        //    {
        //        var result = await cmd.ExecuteScalarAsync();
        //        if (result is null)
        //            return null;

        //        return Convert.ToInt32(result);
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Database Error", e);
        //    }
        //}


    }
}
