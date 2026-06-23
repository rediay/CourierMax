using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using CourierMax.WebApi.Middleware;

namespace CourierMax.Tests.WebApi.Middleware;

public class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CallsNextAndLogsRequestOutcome()
    {
        var mockLogger = new Mock<ILogger<RequestLoggingMiddleware>>();
        var nextCalled = false;

        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        var middleware = new RequestLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/shipments";

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GET") && v.ToString()!.Contains("200")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_PropagatesExceptionsFromNext()
    {
        var mockLogger = new Mock<ILogger<RequestLoggingMiddleware>>();

        RequestDelegate next = _ => throw new InvalidOperationException("boom");

        var middleware = new RequestLoggingMiddleware(next, mockLogger.Object);
        var context = new DefaultHttpContext();

        Func<Task> act = () => middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
