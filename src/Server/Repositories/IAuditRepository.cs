using SPQC.Confer.SelfHosted.Server.Models;

namespace SPQC.Confer.SelfHosted.Server.Repositories;

public interface IAuditRepository : IRepository<AuditEvent>
{
    Task<IReadOnlyList<AuditEvent>> GetByRoomAsync(string roomId, CancellationToken ct = default);
    Task<IReadOnlyList<AuditEvent>> GetByMemberAsync(string memberId, CancellationToken ct = default);
}
