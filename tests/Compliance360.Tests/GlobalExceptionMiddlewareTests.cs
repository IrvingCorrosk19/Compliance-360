using Compliance360.Web.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Compliance360.Tests;

public sealed class GlobalExceptionMiddlewareTests
{
    [Fact]
    public async Task Unauthorized_access_is_reported_as_403()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = Create(_ => throw new UnauthorizedAccessException("Tenant mismatch."));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task Duplicate_database_write_is_reported_as_409()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var postgres = new PostgresException(
            "duplicate key value violates unique constraint",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation);
        var middleware = Create(_ => throw new DbUpdateException("Duplicate.", postgres));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task Aborted_request_is_not_reported_as_5xx()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var context = new DefaultHttpContext
        {
            RequestAborted = cancellation.Token
        };
        var middleware = Create(_ => throw new OperationCanceledException(cancellation.Token));

        await middleware.InvokeAsync(context);

        Assert.Equal(499, context.Response.StatusCode);
    }

    private static GlobalExceptionMiddleware Create(RequestDelegate next) =>
        new(next, NullLogger<GlobalExceptionMiddleware>.Instance);
}
