using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConferRecovery.Server.Models;

public sealed class Member
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public string DisplayName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string ChapterId { get; set; } = default!;

    public MemberRole Role { get; set; } = MemberRole.Listener;
    public MemberStatus Status { get; set; } = MemberStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? ConsentAcknowledgedAt { get; set; }
    public string? ConsentVersion { get; set; }
}

public enum MemberRole
{
    Listener,
    Presenter,
    Host,
    ChapterAdmin,
    OrgAdmin
}

public enum MemberStatus
{
    Pending,
    Active,
    Suspended,
    Removed
}
