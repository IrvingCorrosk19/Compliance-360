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
}
