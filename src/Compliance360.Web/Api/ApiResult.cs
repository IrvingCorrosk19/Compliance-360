using Compliance360.Shared;

namespace Compliance360.Web.Api;

public static class ApiResult
{
    public static IResult From(Result result)
    {
        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    public static IResult From<T>(Result<T> result)
    {
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
    }

    public static IResult FromWorkflowV2<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var error = result.Error ?? "Workflow operation failed.";
        var status = error.Contains("Revision conflict", StringComparison.OrdinalIgnoreCase)
            ? StatusCodes.Status409Conflict
            : error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;

        return Results.Problem(error, statusCode: status);
    }
}
