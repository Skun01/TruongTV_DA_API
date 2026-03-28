using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ApiBehaviorExtension
{
    public static IServiceCollection AddApiBehaviorExtension(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = ApiResponse<object>.FailResponse("Validation_400", 400);
                response.Data = errors;

                // Keep business/validation errors in a unified 200 response shape.
                return new OkObjectResult(response);
            };
        });

        return services;
    }
}