using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Services;

public interface IAuditService
{
    Task RecordAsync(string roomId, string memberId, string memberDisplayName,
        AuditEventType eventType, Dictionary<string, string>? metadata = null,
        CancellationToken ct = default);
}
