using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Common;
using Domain.Constants;
using FluentValidation;

namespace API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Keep business-level failures in HTTP 200 to match current API contract.
        var httpStatus = StatusCodes.Status200OK;
        var response = exception switch
        {
            AppException appEx => BuildAppExceptionResponse(appEx),
            ValidationException validationEx => BuildValidationResponse(validationEx),
            ApplicationException appEx => ApiResponse<object>.FailResponse(appEx.Message, ResolveStatusCode(appEx.Message)),
            UnauthorizedAccessException unauthorizedEx => ApiResponse<object>.FailResponse(unauthorizedEx.Message, 401),
            KeyNotFoundException notFoundEx => ApiResponse<object>.FailResponse(notFoundEx.Message, 404),
            _ => BuildInternalErrorResponse(exception, ref httpStatus)
        };

        context.Response.StatusCode = httpStatus;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static ApiResponse<object> BuildAppExceptionResponse(AppException exception)
    {
        var response = ApiResponse<object>.FailResponse(exception.ErrorCode, exception.StatusCode);
        response.Data = exception.Details;
        return response;
    }

    private ApiResponse<object> BuildInternalErrorResponse(Exception exception, ref int httpStatus)
    {
        httpStatus = StatusCodes.Status500InternalServerError;
        _logger.LogError(exception, "Unhandled exception occurred while processing request");
        return ApiResponse<object>.FailResponse(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR, 500);
    }

    private static ApiResponse<object> BuildValidationResponse(ValidationException validationEx)
    {
        var response = ApiResponse<object>.FailResponse("Validation_400", 400);
        response.Data = validationEx.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return response;
    }

    private static int ResolveStatusCode(string message)
    {
        var match = Regex.Match(message, @"_(\d{3})$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var code))
            return code;

        return 400;
    }
}
