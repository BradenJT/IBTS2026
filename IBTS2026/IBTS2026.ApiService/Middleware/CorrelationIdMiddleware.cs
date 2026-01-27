using System.Diagnostics;

namespace IBTS2026.ApiService.Middleware;

/// <summary>
/// Middleware that manages correlation IDs for request tracing.
/// Uses existing trace ID from Activity.Current if available (from OpenTelemetry),
/// otherwise falls back to X-Correlation-Id header or generates a new one.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prefer the trace ID from OpenTelemetry Activity if available
        var correlationId = Activity.Current?.TraceId.ToString();

        // Fallback to header or generate new
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                ?? Guid.NewGuid().ToString("N");
        }

        // Add correlation ID to response headers for client tracing
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Add correlation ID to logging scope for all downstream logs
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path.ToString(),
            ["RequestMethod"] = context.Request.Method
        }))
        {
            await _next(context);
        }
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
