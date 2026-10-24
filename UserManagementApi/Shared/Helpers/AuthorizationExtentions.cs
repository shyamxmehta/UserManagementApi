using UserManagementApi.Data.Seeders;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UserManagementApi.Shared.Helpers
{
    public static class AuthorizationExtentions
    {
        public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                AddRightsPolicies(options);
            });
            return services;
        }

        public static void AddRightsPolicies(AuthorizationOptions options)
        {
            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(permission, policy => policy.RequireAssertion(context =>
                context.User.HasClaim("Permission", permission) ||
                context.User.HasClaim("RolePermission", permission)
                ));
            }
        }

   
        //public static bool HasPermissionOrAdmin(ClaimsPrincipal user, string permission)
        //{
        //    return user.IsInRole("Admin") ||
        //            user.HasClaim("Permission", permission);
        //}
    }
}
