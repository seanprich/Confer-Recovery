using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Services;

public sealed record LiveKitGrants(
    bool RoomJoin,
    string Room,
    bool CanPublish,
    bool CanSubscribe,
    bool CanPublishData,
    bool Hidden = false);

public interface ILiveKitTokenService
{
    /// <summary>
    /// Issues a short-lived, room-scoped LiveKit token. Every call produces a unique JTI
    /// so tokens cannot be reused even if shared.
    /// </summary>
    Task<string> IssueRoomTokenAsync(Member member, Room room, Chapter chapter,
        CancellationToken ct = default);
}
