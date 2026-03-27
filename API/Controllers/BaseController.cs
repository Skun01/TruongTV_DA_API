using System.Security.Claims;
using System.Text.RegularExpressions;
using Application.Common;
using Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace API.Controllers;

public class BaseController : ControllerBase
{
    protected async Task<ApiResponse<T>> HandleException<T>(Task<T> task)
    {
        try
        {
            var data = await task;
            return ApiResponse<T>.SuccessResponse(data);
        }
        catch(ApplicationException ex)
        {
            return ApiResponse<T>.FailResponse(ex.Message, ResolveStatusCode(ex.Message));
        }
        catch(UnauthorizedAccessException ex)
        {
            return ApiResponse<T>.FailResponse(ex.Message, 401);
        }
        catch(KeyNotFoundException ex)
        {
            return ApiResponse<T>.FailResponse(ex.Message, 404);
        }
        catch (Exception ex)
        {
            Log.Logger.Error($"Failed: {ex.Message}\n{ex.StackTrace}");
            return ApiResponse<T>.FailResponse(MessageConstants.CommonMessage.INTERNAL_SERVER_ERROR, 500);
        }
    }

    protected string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return userId != null ? userId : throw new Exception("Token Không chứa user id");
    }

    private static int ResolveStatusCode(string message)
    {
        var match = Regex.Match(message, @"_(\d{3})$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var code))
            return code;

        return 400;
    }
}
