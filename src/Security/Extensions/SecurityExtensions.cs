using System.Threading.RateLimiting;
using ConferRecovery.Security.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace ConferRecovery.Security.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddConferSecurity(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Named policy for explicit opt-in on specific endpoints if needed
            options.AddFixedWindowLimiter("auth", o =>
            {
                o.PermitLimit = 10;
                o.Window = TimeSpan.FromMinutes(1);
                o.QueueLimit = 0;
            });

            // Global per-IP limiter — auth paths get a much tighter budget
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            {
                var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "anon";
                var isAuthPath = ctx.Request.Path.StartsWithSegments(
                    "/api/auth", StringComparison.OrdinalIgnoreCase);

                return isAuthPath
                    ? RateLimitPartition.GetFixedWindowLimiter($"auth:{ip}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        })
                    : RateLimitPartition.GetFixedWindowLimiter($"api:{ip}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 300,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }

    public static IApplicationBuilder UseConferSecurity(this IApplicationBuilder app)
    {
        app.UseRateLimiter();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<RequestValidationMiddleware>();
        return app;
    }
}
