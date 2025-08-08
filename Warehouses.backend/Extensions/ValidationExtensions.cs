using Microsoft.AspNetCore.Mvc;

namespace Warehouses.backend;

public static class ValidationExtensions
{
    public static void ConfigureValidation(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Any() == true)
                        .ToDictionary(
                            e => e.Key,
                            e => e.Value!.Errors.Select(error => error.ErrorMessage).ToArray()
                        );
                    
                    return new BadRequestObjectResult(new
                    {
                        Message = "Ошибки валидации",
                        Errors = errors
                    });
                };
            });
    }
}