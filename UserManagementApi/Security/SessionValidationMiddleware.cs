using UserManagementApi.Shared.Helpers;
using System.Security.Claims;

namespace UserManagementApi.Security
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
        {
            var token = context.Request.Cookies["SessionToken"];
            var username = context.User.FindFirst(ClaimTypes.Name)?.Value;

            // Only validate authenticated requests that have a session cookie
            if (context.User.Identity?.IsAuthenticated == true
                && token != null
                && username != null)
            {
                var isValid = await sessionService.ValidateSessionAsync(username, token);

                if (!isValid)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Session expired or logged in from another device."
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}
