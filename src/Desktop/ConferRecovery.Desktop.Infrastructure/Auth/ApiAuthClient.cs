using ConferRecovery.Desktop.Application.Auth;
using ConferRecovery.Desktop.Infrastructure.Generated;
using ContractLoginRequest = ConferRecovery.Desktop.Contracts.Auth.LoginRequest;
using ContractLoginResponse = ConferRecovery.Desktop.Contracts.Auth.LoginResponse;
using GeneratedLoginRequest = ConferRecovery.Desktop.Infrastructure.Generated.LoginRequest;

namespace ConferRecovery.Desktop.Infrastructure.Auth;

public sealed class ApiAuthClient : IAuthApiClient
{
    private readonly IAuthClient _client;

    public ApiAuthClient(IAuthClient client)
    {
        _client = client;
    }

    public async Task<ContractLoginResponse?> LoginAsync(ContractLoginRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _client.LoginWithCredentialsAsync(
                new GeneratedLoginRequest
                {
                    Email = request.Email,
                    Password = request.Password
                },
                ct);

            return new ContractLoginResponse(
                response.AccessToken,
                response.ExpiresAt.UtcDateTime,
                response.MemberId,
                response.DisplayName,
                response.ChapterId,
                response.Role);
        }
        catch (ConferApiException ex) when (ex.StatusCode == 401)
        {
            return null;
        }
    }
}