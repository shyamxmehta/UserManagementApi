using UserManagementApi.Modules.Authentication.Filters;
using UserManagementApi.Shared.DTOs;
using UserManagementApi.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Text.Json.Serialization;

public static class ApiBehaviorExtensions
{
    public static IMvcBuilder AddApiControllers(this IServiceCollection services)
    {
        return services
            .AddControllers(options =>
            {
                options.Filters.Add<ApiResponseFilter>();
                options.Filters.Add(new AuthorizeFilter());
                options.Filters.AddService<AntiforgeryValidationFilter>();
            });
           
    }

    //remove nulls from DTO
    public static IMvcBuilder AddApiJsonOptions(this IMvcBuilder builder)
    {
        return builder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                JsonIgnoreCondition.WhenWritingNull;
        });
    }

    public static IServiceCollection AddCustomModelValidation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = new ApiResponseDTO<object>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Validation failed",
                    Errors = errors
                };

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }

}