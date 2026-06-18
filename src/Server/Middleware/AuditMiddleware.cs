namespace ConferRecovery.Server.Middleware;

/// <summary>
/// Logs every authenticated request at Info level — path, method, caller identity.
/// No request/response bodies are ever logged (privacy by design).
/// </summary>
public sealed class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var memberId = context.User.FindFirst("sub")?.Value ?? "unknown";
            _logger.LogInformation(
                "Request: {Method} {Path} → {Status} by member={MemberId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                memberId);
        }
    }
}
