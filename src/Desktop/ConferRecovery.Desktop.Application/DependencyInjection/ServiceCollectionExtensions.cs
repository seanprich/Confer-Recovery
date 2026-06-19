using ConferRecovery.Desktop.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace ConferRecovery.Desktop.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopApplication(this IServiceCollection services)
    {
        services.AddSingleton<AuthenticationService>();
        return services;
    }
}
