namespace ConferRecovery.Desktop.Contracts.Chapters;

public sealed record ChapterSummary(
    string Id,
    string Name,
    string SfuUrl,
    string LiveKitApiKey,
    string Status,
    DateTime CreatedAt);