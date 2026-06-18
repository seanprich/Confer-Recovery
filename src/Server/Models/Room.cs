using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SPQC.Confer.SelfHosted.Server.Models;

public sealed class Room
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string ChapterId { get; set; } = default!;

    public string Name { get; set; } = default!;

    /// <summary>Globally unique name passed to LiveKit — never reused across sessions</summary>
    public string LiveKitRoomName { get; set; } = default!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string HostMemberId { get; set; } = default!;

    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public RoomStatus Status { get; set; } = RoomStatus.Scheduled;
    public bool LobbyEnabled { get; set; } = true;
    public int MaxVideoPublishers { get; set; } = 10;
    public int MaxParticipants { get; set; } = 110;
}

public enum RoomStatus
{
    Scheduled,
    Active,
    Ended,
    Cancelled
}
