namespace ConferRecovery.Desktop.Contracts.Rooms;

public sealed record CreateRoomRequest(
    string Name,
    string ChapterId,
    DateTime? ScheduledAt = null);