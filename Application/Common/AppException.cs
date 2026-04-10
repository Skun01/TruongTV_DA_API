namespace Application.Common;

public class AppException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public object? Details { get; }

    public AppException(string errorCode, int statusCode, string? message = null, object? details = null)
        : base(message ?? errorCode)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Details = details;
    }
}
