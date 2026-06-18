using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Services;

public sealed record ApiToken(string Value, DateTime ExpiresAt);

public interface IApiTokenService
{
    ApiToken Issue(Member member);
}
