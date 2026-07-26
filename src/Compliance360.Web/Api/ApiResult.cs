using Compliance360.Shared;

namespace Compliance360.Web.Api;

public static class ApiResult
{
    public static IResult From(Result result)
    {
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error, statusCode: StatusFromError(result.Error));
    }

    public static IResult From<T>(Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error, statusCode: StatusFromError(result.Error));
    }

    public static IResult FromWorkflowV2<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return Results.Problem(result.Error ?? "Workflow operation failed.", statusCode: StatusFromError(result.Error));
    }

    private static int StatusFromError(string? error)
    {
        var message = error ?? string.Empty;
        if (message.Contains("Revision conflict", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCodes.Status409Conflict;
        }

        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCodes.Status404NotFound;
        }

        if (message.Contains("denied", StringComparison.OrdinalIgnoreCase)
            || message.Contains("forbidden", StringComparison.OrdinalIgnoreCase)
            || message.Contains("permission", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCodes.Status403Forbidden;
        }

        return StatusCodes.Status400BadRequest;
    }
}
