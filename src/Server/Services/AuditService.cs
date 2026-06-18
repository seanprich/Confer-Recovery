using SPQC.Confer.SelfHosted.Server.Models;
using SPQC.Confer.SelfHosted.Server.Repositories;
using SPQC.Confer.SelfHosted.Server.Telemetry;

namespace SPQC.Confer.SelfHosted.Server.Services;

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _audit;
    private readonly ILogger<AuditService> _logger;
    private readonly IConferMetrics _metrics;

    public AuditService(IAuditRepository audit, ILogger<AuditService> logger, IConferMetrics metrics)
    {
        _audit = audit;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task RecordAsync(string roomId, string memberId, string memberDisplayName,
        AuditEventType eventType, Dictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
        var ev = new AuditEvent
        {
            RoomId = roomId,
            MemberId = memberId,
            MemberDisplayName = memberDisplayName,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata
        };

        await _audit.InsertAsync(ev, ct);
        _metrics.AuditEventRecorded(eventType.ToString());
        _logger.LogInformation("Audit: {EventType} member={MemberId} room={RoomId}", eventType, memberId, roomId);
    }
}
