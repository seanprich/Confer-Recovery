using ConferRecovery.Desktop.Application.Auth;
using ConferRecovery.Desktop.Application.Chapters;
using ConferRecovery.Desktop.Application.Members;
using ConferRecovery.Desktop.Application.Rooms;
using ConferRecovery.Desktop.Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ConferRecovery.Tests.Desktop.Infrastructure;

public sealed class DependencyInjectionServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDesktopInfrastructure_RegistersApiClientsAndAdapters()
    {
        var services = new ServiceCollection();

        services.AddDesktopInfrastructure("http://localhost:5000/");

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IAuthApiClient>());
        Assert.NotNull(provider.GetService<IAuthenticatedSessionStore>());
        Assert.NotNull(provider.GetService<IChaptersApiClient>());
        Assert.NotNull(provider.GetService<IMembersApiClient>());
        Assert.NotNull(provider.GetService<IRoomsApiClient>());
    }

    [Fact]
    public void AddDesktopInfrastructure_ConfiguresHttpClientBaseAddress()
    {
        var services = new ServiceCollection();

        services.AddDesktopInfrastructure("http://localhost:5000/");

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<HttpClient>();

        Assert.Equal("http://localhost:5000/", client.BaseAddress?.ToString());
    }
}
