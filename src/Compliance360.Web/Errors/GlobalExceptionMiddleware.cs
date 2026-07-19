using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        catch (BadHttpRequestException exception)
        {
            await WriteProblemAsync(httpContext, StatusCodes.Status400BadRequest, "Malformed request", "The request body could not be read or is not valid.");
            _logger.LogWarning(exception, "Rejected malformed request body.");
        }
        catch (ArgumentException exception)
        {
            await WriteProblemAsync(httpContext, StatusCodes.Status400BadRequest, "Validation error", exception.Message);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected or canceled navigation. This is not an
            // application failure and must not pollute production 5xx signals.
            if (!httpContext.Response.HasStarted)
            {
                httpContext.Response.StatusCode = 499;
            }
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            })
        {
            _logger.LogWarning(exception, "Rejected a duplicate persistence operation.");
            await WriteProblemAsync(
                httpContext,
                StatusCodes.Status409Conflict,
                "Conflict",
                "A record with the same unique identity already exists.");
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
