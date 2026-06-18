using MongoDB.Driver;
using SPQC.Confer.SelfHosted.Server.Models;

namespace SPQC.Confer.SelfHosted.Server.Repositories;

public sealed class AuditRepository : MongoRepository<AuditEvent>, IAuditRepository
{
    public AuditRepository(IMongoDatabase database) : base(database, "audit_events")
    {
        var roomIndex = new CreateIndexModel<AuditEvent>(
            Builders<AuditEvent>.IndexKeys.Ascending(a => a.RoomId).Descending(a => a.Timestamp));
        var memberIndex = new CreateIndexModel<AuditEvent>(
            Builders<AuditEvent>.IndexKeys.Ascending(a => a.MemberId).Descending(a => a.Timestamp));
        Collection.Indexes.CreateMany([roomIndex, memberIndex]);
    }

    public async Task<IReadOnlyList<AuditEvent>> GetByRoomAsync(string roomId, CancellationToken ct = default)
        => await FindAsync(a => a.RoomId == roomId, ct);

    public async Task<IReadOnlyList<AuditEvent>> GetByMemberAsync(string memberId, CancellationToken ct = default)
        => await FindAsync(a => a.MemberId == memberId, ct);
}
