namespace UserManagementApi.Data.Seeders
{
    public static class Permissions
    {
        public const string ViewUsers = "VIEW_USERS";
        public const string CreateUser = "CREATE_USER";
        public const string UpdateUser = "UPDATE_USER";
        public const string DeleteUser = "DELETE_USER";

        public static readonly string[] All =
        {
            ViewUsers,
            CreateUser,
            UpdateUser,
            DeleteUser,
        };
    }
}
