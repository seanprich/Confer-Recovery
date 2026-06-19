using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using ConferRecovery.Security.Middleware;

namespace ConferRecovery.Tests.Middleware;

public sealed class RequestValidationMiddlewareTests
{
    private static (RequestValidationMiddleware Sut, Func<bool> NextWasCalled) Build()
    {
        var called = false;
        var sut = new RequestValidationMiddleware(
            _ => { called = true; return Task.CompletedTask; },
            NullLogger<RequestValidationMiddleware>.Instance);
        return (sut, () => called);
    }

    // ── Content-Type enforcement on write methods ─────────────────────────────

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_WriteMethodWithNonJsonBody_Returns415(string method)
    {
        var (sut, nextCalled) = Build();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = method;
        ctx.Request.ContentType = "text/plain";
        ctx.Request.ContentLength = 10;

        await sut.InvokeAsync(ctx);

        Assert.Equal(StatusCodes.Status415UnsupportedMediaType, ctx.Response.StatusCode);
        Assert.False(nextCalled());
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_WriteMethodWithJsonBody_PassesThrough(string method)
    {
        var (sut, nextCalled) = Build();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = method;
        ctx.Request.ContentType = "application/json";
        ctx.Request.ContentLength = 10;

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled());
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_WriteMethodWithEmptyBody_PassesThrough(string method)
    {
        // Empty body (ContentLength = 0) should not require Content-Type
        var (sut, nextCalled) = Build();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = method;
        ctx.Request.ContentType = "text/plain";
        ctx.Request.ContentLength = 0;

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled());
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("DELETE")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    public async Task InvokeAsync_NonWriteMethod_PassesThroughRegardlessOfContentType(string method)
    {
        var (sut, nextCalled) = Build();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = method;
        ctx.Request.ContentType = "text/xml";

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled());
    }

    [Fact]
    public async Task InvokeAsync_JsonContentTypeWithCharset_PassesThrough()
    {
        // application/json; charset=utf-8 must be accepted
        var (sut, nextCalled) = Build();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.ContentType = "application/json; charset=utf-8";
        ctx.Request.ContentLength = 10;

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled());
    }

    // ── Query string injection detection ─────────────────────────────────────

    [Theory]
    [InlineData("$where")]
    [InlineData("$function")]
    [InlineData("$accumulator")]
    [InlineData("$expr")]
    [InlineData("this.")]
    public async Task InvokeAsync_QueryStringWithDangerousPattern_Returns400(string pattern)
    {
        var (sut, nextCalled) = Build();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.QueryString = new QueryString($"?filter={pattern}password");

        await sut.InvokeAsync(ctx);

        Assert.Equal(StatusCodes.Status400BadRequest, ctx.Response.StatusCode);
        Assert.False(nextCalled());
    }

    [Fact]
    public async Task InvokeAsync_DangerousPatternIsCaseInsensitive()
    {
        var (sut, nextCalled) = Build();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.QueryString = new QueryString("?x=$WHERE(true)");

        await sut.InvokeAsync(ctx);

        Assert.Equal(StatusCodes.Status400BadRequest, ctx.Response.StatusCode);
        Assert.False(nextCalled());
    }

    [Fact]
    public async Task InvokeAsync_CleanQueryString_PassesThrough()
    {
        var (sut, nextCalled) = Build();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.QueryString = new QueryString("?status=Active&page=1");

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled());
    }

    [Fact]
    public async Task InvokeAsync_NoQueryString_PassesThrough()
    {
        var (sut, nextCalled) = Build();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled());
    }
}
