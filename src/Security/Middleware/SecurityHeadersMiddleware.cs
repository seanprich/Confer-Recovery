using Microsoft.AspNetCore.Http;

namespace ConferRecovery.Security.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.Remove("Server");
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        // Pure JSON API — no scripts, frames, or external resources
        headers["Content-Security-Policy"] = "default-src 'none'";

        await _next(context);
    }
}
