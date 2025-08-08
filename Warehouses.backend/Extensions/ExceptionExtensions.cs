using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Warehouses.backend.Exceptions;

namespace Warehouses.backend;

public static class ExceptionExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var ex = contextFeature.Error;
                    string message;
                    
                    switch (ex)
                    {
                        case NotFoundException:
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            message = ex.Message;
                            break;
                            
                        case InvalidOperationException:
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            message = ex.Message;
                            break;
                            
                        default:
                            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError(ex, "Необработанное исключение");
                            message = "Внутренняя ошибка сервера";
                            break;
                    }
                    
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = message
                    }));
                }
            });
        });
    }
}