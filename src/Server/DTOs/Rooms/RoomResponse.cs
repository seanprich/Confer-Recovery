namespace SPQC.Confer.SelfHosted.Server.DTOs.Rooms;

public sealed record RoomResponse(
    string Id,
    string ChapterId,
    string Name,
    string HostMemberId,
    string Status,
    DateTime? ScheduledAt,
    DateTime? StartedAt,
    DateTime? EndedAt,
    bool LobbyEnabled,
    int MaxVideoPublishers,
    int MaxParticipants);
