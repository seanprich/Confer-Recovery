using ConferRecovery.Desktop.Contracts.Chapters;

namespace ConferRecovery.Desktop.Application.Chapters;

public interface IChaptersApiClient
{
    Task<IReadOnlyList<ChapterSummary>> GetActiveAsync(CancellationToken ct);
    Task<ChapterSummary?> GetByIdAsync(string id, CancellationToken ct);
    Task<ChapterSummary> CreateAsync(CreateChapterRequest request, CancellationToken ct);
    Task<bool> UpdateSfuAsync(string id, CreateChapterRequest request, CancellationToken ct);
    Task<bool> SetStatusAsync(string id, string status, CancellationToken ct);
}