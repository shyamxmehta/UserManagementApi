using UserManagementApi.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace UserManagementApi.Shared.Helpers
{
    public class ApiExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JsonSerializerOptions _jsonOptions;
        public ApiExceptionMiddleware(RequestDelegate next, IOptions<JsonOptions> options)
        {
            _next = next;
            _jsonOptions = options.Value.JsonSerializerOptions;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var statusCode = ex switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    InvalidOperationException => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var response = new ApiResponseDTO<object?>
                {
                    Success = false,
                    StatusCode = statusCode,
                    Message = ex.Message
                };

                await context.Response.WriteAsJsonAsync(response, _jsonOptions);
            }
        }
    }
}
