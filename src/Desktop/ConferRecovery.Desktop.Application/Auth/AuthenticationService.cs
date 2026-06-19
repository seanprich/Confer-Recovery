using ConferRecovery.Desktop.Contracts.Auth;
using ConferRecovery.Desktop.Contracts.Session;

namespace ConferRecovery.Desktop.Application.Auth;

public sealed class AuthenticationService
{
    private readonly IAuthApiClient _apiClient;
    private readonly IAuthenticatedSessionStore _sessionStore;

    public AuthenticationService(IAuthApiClient apiClient, IAuthenticatedSessionStore sessionStore)
    {
        _apiClient = apiClient;
        _sessionStore = sessionStore;
    }

    public async Task<AuthenticatedSession?> LoginAsync(LoginAttempt attempt, CancellationToken ct)
    {
        var request = new LoginRequest(attempt.Email.Trim(), attempt.Password);
        var response = await _apiClient.LoginAsync(request, ct);
        if (response is null)
        {
            return null;
        }

        var session = new AuthenticatedSession(
            response.AccessToken,
            response.ExpiresAt,
            response.MemberId,
            response.DisplayName,
            response.ChapterId,
            response.Role);

        _sessionStore.Set(session);
        return session;
    }
}