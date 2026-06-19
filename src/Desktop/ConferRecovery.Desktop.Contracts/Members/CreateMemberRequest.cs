namespace ConferRecovery.Desktop.Contracts.Members;

public sealed record CreateMemberRequest(
    string DisplayName,
    string Email,
    string Password,
    string ChapterId,
    string Role = "Listener");