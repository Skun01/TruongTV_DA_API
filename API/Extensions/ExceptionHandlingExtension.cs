using API.Middlewares;

namespace API.Extensions;

public static class ExceptionHandlingExtension
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }
}