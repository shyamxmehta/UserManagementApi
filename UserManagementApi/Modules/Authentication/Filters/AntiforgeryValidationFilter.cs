using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UserManagementApi.Modules.Authentication.Filters
{
    public class AntiforgeryValidationFilter : IAsyncActionFilter
    {
        private readonly IAntiforgery _antiforgery;

        public AntiforgeryValidationFilter(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check for IgnoreAntiforgeryToken on action or controller
            var hasIgnoreAttribute = context.ActionDescriptor
                .EndpointMetadata
                .OfType<IgnoreAntiforgeryTokenAttribute>()
                .Any();

            if (hasIgnoreAttribute)
            {
                await next();
                return;
            }

            // Skip safe methods
            var method = context.HttpContext.Request.Method;
            if (HttpMethods.IsGet(method) ||
                HttpMethods.IsHead(method) ||
                HttpMethods.IsOptions(method) ||
                HttpMethods.IsTrace(method)) 
            {
                await next();
                return;
            }

            try
            {
                await _antiforgery.ValidateRequestAsync(context.HttpContext);
            }
            catch (Exception ex)
            {
                throw new AntiforgeryValidationException(ex.Message);
            }

            await next();
        }
    }
}
