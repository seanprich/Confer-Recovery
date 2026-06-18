using SPQC.Confer.SelfHosted.Server.Models;

namespace SPQC.Confer.SelfHosted.Server.Services;

public sealed record ApiToken(string Value, DateTime ExpiresAt);

public interface IApiTokenService
{
    ApiToken Issue(Member member);
}
