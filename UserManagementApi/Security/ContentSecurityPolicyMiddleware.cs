namespace UserManagementApi.Security
{
    public class ContentSecurityPolicyMiddleware(RequestDelegate _next)
    {
        public async Task InvokeAsync(
            HttpContext context)
        {
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                // Allow Scalar's scripts and the specific inline hash you have
                "script-src 'self' 'unsafe-eval' https://jsdelivr.net 'sha256-4K/854y8OleJcGPnN2HGg2GNOXHJVi3NXIepGNyvgvk='; " +
                // Allow Scalar's dynamic styling
                "style-src 'self' 'unsafe-inline'; " +
                // Fix the latest errors: Allow Scalar's fonts
                "font-src 'self' https://fonts.scalar.com; " +
                // Keep dev tools (Hot Reload/Browser Link) working
                "connect-src 'self' https://api.scalar.com http://localhost:* ws://localhost:* https://localhost:*; " +
                "img-src 'self' data:;";

            await _next(context);
        }
    }
}
