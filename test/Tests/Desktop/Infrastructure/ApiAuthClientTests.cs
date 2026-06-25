using ConferRecovery.Desktop.Infrastructure.Auth;
using ConferRecovery.Desktop.Infrastructure.Generated;
using NSubstitute;

namespace ConferRecovery.Tests.Desktop.Infrastructure;

public sealed class ApiAuthClientTests
{
    [Fact]
    public async Task LoginAsync_WhenAuthClientReturnsResponse_MapsToContractResponse()
    {
        var generated = Substitute.For<IAuthClient>();
        var expiresAt = new DateTimeOffset(DateTime.UtcNow.AddMinutes(30));

        generated.AuthLoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(new LoginResponse
            {
                AccessToken = "token",
                ExpiresAt = expiresAt,
                MemberId = "member-1",
                DisplayName = "Jane",
                ChapterId = "chapter-1",
                Role = "Host"
            });

        var sut = new ApiAuthClient(generated);

        var result = await sut.LoginAsync(
            new ConferRecovery.Desktop.Contracts.Auth.LoginRequest("email@example.org", "password123"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("token", result!.AccessToken);
        Assert.Equal(expiresAt.UtcDateTime, result.ExpiresAt);
        Assert.Equal("member-1", result.MemberId);
    }

    [Fact]
    public async Task LoginAsync_WhenUnauthorized_ReturnsNull()
    {
        var generated = Substitute.For<IAuthClient>();
        generated.AuthLoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<LoginResponse>>(_ => throw new ConferApiException(
                "Unauthorized",
                401,
                string.Empty,
                new Dictionary<string, IEnumerable<string>>(),
                null!));

        var sut = new ApiAuthClient(generated);

        var result = await sut.LoginAsync(
            new ConferRecovery.Desktop.Contracts.Auth.LoginRequest("email@example.org", "password123"),
            CancellationToken.None);

        Assert.Null(result);
    }
}