using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferRecovery.Server.Models;

/// <summary>
/// Immutable audit record — who did what and when, with zero media content.
/// Insert-only; never updated or deleted.
/// </summary>
public sealed class AuditEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string RoomId { get; set; } = default!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string MemberId { get; set; } = default!;

    /// <summary>Snapshot of DisplayName at event time — survives member renames</summary>
    public string MemberDisplayName { get; set; } = default!;

    public AuditEventType EventType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Optional structured metadata (e.g. consent version, ejection reason)</summary>
    public Dictionary<string, string>? Metadata { get; set; }
}

public enum AuditEventType
{
    TokenIssued,
    ConsentAcknowledged,
    Joined,
    Left,
    Admitted,
    Ejected,
    Muted,
    Unmuted,
    ScreenShareStarted,
    ScreenShareStopped
}
