using Microsoft.AspNetCore.Http;
using ConferRecovery.Security.Middleware;

namespace ConferRecovery.Tests.Middleware;

public sealed class SecurityHeadersMiddlewareTests
{
    private static async Task<HttpContext> Run(Action<DefaultHttpContext>? setup = null)
    {
        var ctx = new DefaultHttpContext();
        setup?.Invoke(ctx);
        await new SecurityHeadersMiddleware(_ => Task.CompletedTask).InvokeAsync(ctx);
        return ctx;
    }

    [Fact]
    public async Task InvokeAsync_SetsXContentTypeOptionsNosniff()
    {
        var ctx = await Run();
        Assert.Equal("nosniff", ctx.Response.Headers["X-Content-Type-Options"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_SetsXFrameOptionsDeny()
    {
        var ctx = await Run();
        Assert.Equal("DENY", ctx.Response.Headers["X-Frame-Options"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_SetsReferrerPolicy()
    {
        var ctx = await Run();
        Assert.Equal("strict-origin-when-cross-origin",
            ctx.Response.Headers["Referrer-Policy"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_SetsPermissionsPolicy()
    {
        var ctx = await Run();
        Assert.Equal("camera=(), microphone=(), geolocation=()",
            ctx.Response.Headers["Permissions-Policy"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_SetsContentSecurityPolicy()
    {
        var ctx = await Run();
        Assert.Equal("default-src 'none'",
            ctx.Response.Headers["Content-Security-Policy"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_RemovesServerHeader()
    {
        var ctx = await Run(c => c.Response.Headers["Server"] = "Kestrel");
        Assert.False(ctx.Response.Headers.ContainsKey("Server"));
    }

    [Fact]
    public async Task InvokeAsync_AlwaysCallsNext()
    {
        var nextCalled = false;
        var ctx = new DefaultHttpContext();
        await new SecurityHeadersMiddleware(_ => { nextCalled = true; return Task.CompletedTask; })
            .InvokeAsync(ctx);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_SetsHeadersBeforeNextRuns()
    {
        // Ensures headers are written before downstream middleware sees the response
        string? cspWhenNextRuns = null;
        var ctx = new DefaultHttpContext();
        await new SecurityHeadersMiddleware(c =>
        {
            cspWhenNextRuns = c.Response.Headers["Content-Security-Policy"].ToString();
            return Task.CompletedTask;
        }).InvokeAsync(ctx);
        Assert.Equal("default-src 'none'", cspWhenNextRuns);
    }
}
