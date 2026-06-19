namespace ConferRecovery.Server.DTOs.Members;

public sealed record MemberResponse(
    string Id,
    string DisplayName,
    string Email,
    string ChapterId,
    string Role,
    string Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool ConsentAcknowledged);
