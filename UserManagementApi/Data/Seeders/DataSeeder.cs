using UserManagementApi.Data.Access;
using UserManagementApi.Data.Mappers;
using UserManagementApi.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace UserManagementApi.Data.Seeders
{
    public class DataSeeder
    {

        private readonly IConnectionFactory _connection;
        private readonly IProtectData _protectData;
        public DataSeeder(IConnectionFactory connection, IProtectData protectData)
        {
            _connection = connection;
            _protectData = protectData;
        }

        public async Task SeedAsync()
        {
            using var conn = await _connection.CreateOpenSqlConnection();

            await SeedRoles(conn);
            await SeedPermissions(conn);
            await SeedAdminUser(conn);
            await SeedAdminRights(conn);
        }

        private async Task SeedRoles(SqlConnection conn)
        {
            var sql = @"
                    IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Admin')
                    BEGIN
                        INSERT INTO Roles (Name) VALUES ('Admin')
                    END

                    IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'User')
                    BEGIN
                        INSERT INTO Roles (Name) VALUES ('User')
                    END";

            using var cmd = new SqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task SeedPermissions(SqlConnection conn)
        {
            foreach (var permission in Permissions.All)
            {
                var sql = @"
                        IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Name = @perm)
                        INSERT INTO Permissions (Name) VALUES (@perm)
                        ";

                using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add("@perm", SqlDbType.NVarChar).Value = permission;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task SeedAdminUser(SqlConnection conn)
        {
            string pass = "0000";
            // Use Encoding.UTF8.GetBytes for plain text input
            byte[] bytes = Encoding.UTF8.GetBytes(pass);

            // Hash the bytes and convert the result to a Base64 string for storage
            string passwordHash = Convert.ToBase64String(SHA256.HashData(bytes));
            var sql = @"
                    IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'Admin')
                        INSERT INTO Users (Username, PasswordHash, Role) VALUES ('Admin', @PasswordHash, 'Admin')
                    ";

            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = passwordHash;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task SeedAdminRights(SqlConnection conn)
        {
            foreach (var permission in Permissions.All)
            {
                byte[] bytes = Encoding.UTF8.GetBytes("Admin" + permission);
                var checkSum = _protectData.Encrypt(Convert.ToBase64String(SHA256.HashData(bytes)));
                
                var sql = @"
                            SELECT [RoleID]
                                  ,[Permission]
                                  ,[Checksum]
                              FROM [DemoAppDb].[dbo].[RolePermissions]
                            INSERT INTO RolePermissions (RoleID, Permission, Checksum)
                            SELECT r.ID, @permission, @checksum
                            FROM Roles r
                            WHERE r.Name = 'Admin'
                            AND NOT EXISTS (
                                SELECT 1 
                                FROM RolePermissions rp
                                WHERE rp.RoleID = r.ID
                                AND rp.Permission = @permission
                            );";

                using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.Add("@permission", SqlDbType.NVarChar).Value = permission;
                cmd.Parameters.Add("@checksum", SqlDbType.NVarChar).Value = checkSum;

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
