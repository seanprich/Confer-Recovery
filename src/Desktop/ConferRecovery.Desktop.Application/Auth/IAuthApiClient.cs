using ConferRecovery.Desktop.Contracts.Auth;

namespace ConferRecovery.Desktop.Application.Auth;

public interface IAuthApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct);
}