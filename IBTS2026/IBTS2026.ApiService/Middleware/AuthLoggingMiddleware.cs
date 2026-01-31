using System.Security.Claims;

namespace IBTS2026.ApiService.Middleware;

public class AuthLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthLoggingMiddleware> _logger;

    public AuthLoggingMiddleware(RequestDelegate next, ILogger<AuthLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        var method = context.Request.Method;

        // Log authentication header presence
        var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");
        var authHeader = hasAuthHeader ? context.Request.Headers["Authorization"].ToString() : "(none)";

        _logger.LogInformation(
            "AUTH DEBUG [{Method}] {Path} - Auth Header Present: {HasAuth}, Header: {AuthHeader}",
            method, path, hasAuthHeader,
            hasAuthHeader ? authHeader.Substring(0, Math.Min(50, authHeader.Length)) + "..." : "(none)");

        // Log user claims after authentication
        if (context.User.Identity?.IsAuthenticated == true)
        {
            _logger.LogInformation("AUTH DEBUG: User IS authenticated. Identity: {Identity}",
                context.User.Identity.Name ?? "(no name)");

            foreach (var claim in context.User.Claims)
            {
                _logger.LogInformation("AUTH DEBUG: Claim Type={Type}, Value={Value}",
                    claim.Type, claim.Value);
            }

            // Specifically check for role claim
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            var roleClaimShort = context.User.FindFirst("role");
            _logger.LogInformation(
                "AUTH DEBUG: Role claim (ClaimTypes.Role): {Role}, Role claim (\"role\"): {RoleShort}",
                roleClaim?.Value ?? "(not found)",
                roleClaimShort?.Value ?? "(not found)");
        }
        else
        {
            _logger.LogWarning("AUTH DEBUG: User is NOT authenticated for {Method} {Path}",
                method, path);
        }

        await _next(context);
    }
}

public static class AuthLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AuthLoggingMiddleware>();
    }
}
