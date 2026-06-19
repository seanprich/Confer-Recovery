using ConferRecovery.Desktop.Application.Auth;
using ConferRecovery.Desktop.Application.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ConferRecovery.Tests.Desktop.Application;

public sealed class DependencyInjectionServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDesktopApplication_RegistersAuthenticationService()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IAuthApiClient>());
        services.AddSingleton(Substitute.For<IAuthenticatedSessionStore>());

        services.AddDesktopApplication();

        using var provider = services.BuildServiceProvider();
        var auth = provider.GetService<AuthenticationService>();

        Assert.NotNull(auth);
    }
}
