using ConferRecovery.Desktop.Application.Auth;
using ConferRecovery.Desktop.Contracts.Session;

namespace ConferRecovery.Desktop.Infrastructure.Auth;

public sealed class InMemoryAuthenticatedSessionStore : IAuthenticatedSessionStore
{
    public AuthenticatedSession? Current { get; private set; }

    public void Set(AuthenticatedSession session)
    {
        Current = session;
    }

    public void Clear()
    {
        Current = null;
    }
}