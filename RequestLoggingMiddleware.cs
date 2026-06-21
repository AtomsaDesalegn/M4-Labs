using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TMSAPI;

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
        string correlationId = Guid.NewGuid().ToString("N")[..8];
        context.Response.Headers["X-Correlation-Id"] = correlationId;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Request {Method} {Path} started with CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Request {Method} {Path} finished with status {StatusCode} in {ElabsedMs}ms (CorrelationId:{CorrelationId})",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}
