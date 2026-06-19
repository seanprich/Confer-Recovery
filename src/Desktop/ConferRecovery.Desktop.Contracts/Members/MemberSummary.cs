namespace ConferRecovery.Desktop.Contracts.Members;

public sealed record MemberSummary(
    string Id,
    string DisplayName,
    string Email,
    string ChapterId,
    string Role,
    string Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool ConsentAcknowledged);