namespace ConferRecovery.Desktop.Contracts.Rooms;

public sealed record JoinRoomResponse(
    string LiveKitToken,
    string SfuUrl,
    string RoomName,
    DateTime TokenExpiresAt);