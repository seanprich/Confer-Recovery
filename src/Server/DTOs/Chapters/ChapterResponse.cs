namespace ConferRecovery.Server.DTOs.Chapters;

public sealed record ChapterResponse(
    string Id,
    string Name,
    string SfuUrl,
    string LiveKitApiKey,
    string Status,
    DateTime CreatedAt);
