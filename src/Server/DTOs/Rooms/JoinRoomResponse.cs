namespace ConferRecovery.Server.DTOs.Rooms;

public sealed record JoinRoomResponse(
    string LiveKitToken,
    string SfuUrl,
    string RoomName,
    DateTime TokenExpiresAt);
