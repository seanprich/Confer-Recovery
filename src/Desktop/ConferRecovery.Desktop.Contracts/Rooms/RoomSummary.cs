namespace ConferRecovery.Desktop.Contracts.Rooms;

public sealed record RoomSummary(
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