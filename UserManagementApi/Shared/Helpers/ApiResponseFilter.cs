using UserManagementApi.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UserManagementApi.Shared.Helpers
{
    public class ApiResponseFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is not ObjectResult objectResult)
                return;

            // Skip already wrapped
            if (IsAlreadyWrapped(objectResult.Value))
                return;

            // Skip file/stream responses
            if (context.Result is FileResult)
                return;

            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

            var isSuccess = statusCode >= 200 && statusCode < 300;

            object? data = null;
            object? errors = null;
            string message = GetDefaultMessage(statusCode);

            var value = objectResult.Value;

            if (isSuccess)
            {
                // SUCCESS FLOW
                data = value is string str ? new { message = str } : value;
            }
            else
            {
                // ERROR FLOW

                switch (value)
                {
                    case ValidationProblemDetails validation:
                        errors = validation.Errors;
                        message = "Validation failed";
                        break;

                    case ProblemDetails problem:
                        message = problem.Title ?? message;
                        errors = problem.Detail;
                        break;

                    case string str:
                        message = str;
                        break;

                    default:
                        errors = value;
                        break;
                }
            }

            var wrapped = new ApiResponseDTO<object>
            {
                Success = isSuccess,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Errors = errors
            };

            context.Result = new ObjectResult(wrapped)
            {
                StatusCode = statusCode
            };
        }

        private static bool IsAlreadyWrapped(object? value)
        {
            if (value == null) return false;

            var type = value.GetType();

            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(ApiResponseDTO<>);
        }

        private static string GetDefaultMessage(int statusCode)
        {
            return statusCode switch
            {
                200 => "Success",
                201 => "Created successfully",
                204 => "No content",
                400 => "Bad request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not found",
                409 => "Conflict",
                500 => "Internal server error",
                _ => "Request processed"
            };
        }

    }

}
