namespace ConferRecovery.Desktop.Contracts.Chapters;

public sealed record CreateChapterRequest(
    string Name,
    string SfuUrl,
    string LiveKitApiKey,
    string LiveKitApiSecret);