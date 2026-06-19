using System.Net.Http;
using ConferRecovery.Desktop.Application.Auth;
using ConferRecovery.Desktop.Application.Chapters;
using ConferRecovery.Desktop.Application.Members;
using ConferRecovery.Desktop.Application.Rooms;
using ConferRecovery.Desktop.Infrastructure.Auth;
using ConferRecovery.Desktop.Infrastructure.Chapters;
using ConferRecovery.Desktop.Infrastructure.Generated;
using ConferRecovery.Desktop.Infrastructure.Members;
using ConferRecovery.Desktop.Infrastructure.Rooms;
using Microsoft.Extensions.DependencyInjection;

namespace ConferRecovery.Desktop.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopInfrastructure(
        this IServiceCollection services,
        string baseAddress)
    {
        services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        });

        services.AddSingleton<IAuthClient>(sp => new AuthClient(sp.GetRequiredService<HttpClient>()));
        services.AddSingleton<IChaptersClient>(sp => new ChaptersClient(sp.GetRequiredService<HttpClient>()));
        services.AddSingleton<IMembersClient>(sp => new MembersClient(sp.GetRequiredService<HttpClient>()));
        services.AddSingleton<IRoomsClient>(sp => new RoomsClient(sp.GetRequiredService<HttpClient>()));

        services.AddSingleton<IAuthApiClient, ApiAuthClient>();
        services.AddSingleton<IAuthenticatedSessionStore, InMemoryAuthenticatedSessionStore>();
        services.AddSingleton<IChaptersApiClient, ApiChaptersClient>();
        services.AddSingleton<IMembersApiClient, ApiMembersClient>();
        services.AddSingleton<IRoomsApiClient, ApiRoomsClient>();

        return services;
    }
}
