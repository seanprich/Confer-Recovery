using ConferRecovery.Desktop.Contracts.Session;

namespace ConferRecovery.Desktop.Application.Auth;

public interface IAuthenticatedSessionStore
{
    AuthenticatedSession? Current { get; }
    void Set(AuthenticatedSession session);
    void Clear();
}