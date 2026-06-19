namespace ConferRecovery.Desktop.Contracts.Session;

// Immutable session model to keep issued token metadata read-only in memory.
public sealed record AuthenticatedSession(
    string AccessToken,
    DateTime ExpiresAt,
    string MemberId,
    string DisplayName,
    string ChapterId,
    string Role);