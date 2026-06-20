namespace Compliance360.Web.Errors;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteProblemAsync(httpContext, StatusCodes.Status403Forbidden, "Forbidden", exception.Message);
        }
        catch (ArgumentException exception)
        {
            await WriteProblemAsync(httpContext, StatusCodes.Status400BadRequest, "Validation error", exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled API exception.");
            await WriteProblemAsync(httpContext, StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext httpContext, int statusCode, string title, string detail)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail,
            traceId = httpContext.TraceIdentifier
        });
    }
}
