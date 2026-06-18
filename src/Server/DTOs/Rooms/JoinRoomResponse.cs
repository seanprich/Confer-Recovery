namespace SPQC.Confer.SelfHosted.Server.DTOs.Rooms;

public sealed record JoinRoomResponse(
    string LiveKitToken,
    string SfuUrl,
    string RoomName,
    DateTime TokenExpiresAt);
