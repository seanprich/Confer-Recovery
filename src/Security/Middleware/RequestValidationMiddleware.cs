using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ConferRecovery.Security.Middleware;

public sealed class RequestValidationMiddleware
{
    private static readonly HashSet<string> WriteMethods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH" };

    // MongoDB operator keywords that should never appear in query strings
    private static readonly string[] DangerousQueryPatterns =
        ["$where", "$function", "$accumulator", "$expr", "this."];

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;

    public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        // Write operations must send JSON
        if (WriteMethods.Contains(request.Method) && request.ContentLength > 0 &&
            !request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            _logger.LogWarning(
                "Rejected {Method} {Path}: unsupported Content-Type '{ContentType}' from {IP}",
                request.Method, request.Path, request.ContentType,
                context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            return;
        }

        // Block query strings containing NoSQL operator patterns
        if (request.QueryString.HasValue &&
            ContainsDangerousPattern(request.QueryString.Value))
        {
            _logger.LogWarning(
                "Blocked suspicious query string from {IP} on {Path}: {Query}",
                context.Connection.RemoteIpAddress, request.Path, request.QueryString.Value);
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        await _next(context);
    }

    private static bool ContainsDangerousPattern(string? value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        foreach (var pattern in DangerousQueryPatterns)
            if (value.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }
}
