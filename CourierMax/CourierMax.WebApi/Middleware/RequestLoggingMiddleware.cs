using System.Diagnostics;

namespace CourierMax.WebApi.Middleware;

/// <summary>
/// Logs method, path, resulting status code and elapsed time for every request.
/// Wraps the whole pipeline (including <see cref="ExceptionHandlingMiddleware"/>)
/// so the logged status code always reflects what the client actually received.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        using (_logger.BeginScope(new Dictionary<string, object> { ["TraceId"] = context.TraceIdentifier }))
        {
            await _next(context);

            stopwatch.Stop();

            _logger.LogInformation(
                "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
