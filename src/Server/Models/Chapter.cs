using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SPQC.Confer.SelfHosted.Server.Models;

public sealed class Chapter
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public string Name { get; set; } = default!;

    /// <summary>wss://your-node.duckdns.org — the LiveKit SFU endpoint for this chapter</summary>
    public string SfuUrl { get; set; } = default!;

    public string LiveKitApiKey { get; set; } = default!;

    /// <summary>Encrypted at rest; never returned in API responses</summary>
    public string LiveKitApiSecretEncrypted { get; set; } = default!;

    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> AdminMemberIds { get; set; } = [];

    public ChapterStatus Status { get; set; } = ChapterStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ChapterStatus
{
    Active,
    Inactive,
    Suspended
}
