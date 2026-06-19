namespace ConferRecovery.Server.DTOs.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string MemberId,
    string DisplayName,
    string ChapterId,
    string Role);
