using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Repositories;

public interface IAuditRepository : IRepository<AuditEvent>
{
    Task<IReadOnlyList<AuditEvent>> GetByRoomAsync(string roomId, CancellationToken ct = default);
    Task<IReadOnlyList<AuditEvent>> GetByMemberAsync(string memberId, CancellationToken ct = default);
}
