using ConferRecovery.Desktop.Application.Auth;
using ConferRecovery.Desktop.Contracts.Auth;
using ConferRecovery.Desktop.Contracts.Session;
using NSubstitute;

namespace ConferRecovery.Tests.Desktop.Application;

public sealed class AuthenticationServiceTests
{
    [Fact]
    public async Task LoginAsync_WhenApiReturnsNull_ReturnsNullAndDoesNotSetSession()
    {
        var api = Substitute.For<IAuthApiClient>();
        var store = Substitute.For<IAuthenticatedSessionStore>();
        api.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns((LoginResponse?)null);

        var sut = new AuthenticationService(api, store);

        var result = await sut.LoginAsync(new LoginAttempt("person@example.org", "password123"), CancellationToken.None);

        Assert.Null(result);
        store.DidNotReceive().Set(Arg.Any<AuthenticatedSession>());
    }

    [Fact]
    public async Task LoginAsync_WhenApiReturnsResponse_MapsAndStoresSession()
    {
        var api = Substitute.For<IAuthApiClient>();
        var store = Substitute.For<IAuthenticatedSessionStore>();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        api.LoginAsync(Arg.Any<LoginRequest>(), Arg.Any<CancellationToken>())
            .Returns(new LoginResponse(
                "token-value",
                expiresAt,
                "member-1",
                "Jane",
                "chapter-1",
                "Host"));

        var sut = new AuthenticationService(api, store);

        var result = await sut.LoginAsync(new LoginAttempt("  person@example.org  ", "password123"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("token-value", result!.AccessToken);
        Assert.Equal(expiresAt, result.ExpiresAt);
        Assert.Equal("member-1", result.MemberId);
        Assert.Equal("Jane", result.DisplayName);
        Assert.Equal("chapter-1", result.ChapterId);
        Assert.Equal("Host", result.Role);

        await api.Received(1).LoginAsync(
            Arg.Is<LoginRequest>(x => x.Email == "person@example.org" && x.Password == "password123"),
            Arg.Any<CancellationToken>());

        store.Received(1).Set(Arg.Is<AuthenticatedSession>(x =>
            x.AccessToken == "token-value" &&
            x.MemberId == "member-1" &&
            x.DisplayName == "Jane"));
    }
}