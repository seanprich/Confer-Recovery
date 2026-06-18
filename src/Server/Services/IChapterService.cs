using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Services;

public interface IChapterService
{
    Task<Chapter?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Chapter>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Chapter> CreateAsync(string name, string sfuUrl, string liveKitApiKey,
        string liveKitApiSecret, CancellationToken ct = default);
    Task<bool> UpdateSfuAsync(string id, string sfuUrl, string liveKitApiKey,
        string liveKitApiSecret, CancellationToken ct = default);
    Task<bool> SetStatusAsync(string id, ChapterStatus status, CancellationToken ct = default);

    /// <summary>Returns the decrypted LiveKit API secret. Never expose this in API responses.</summary>
    Task<string?> GetDecryptedSecretAsync(string chapterId, CancellationToken ct = default);
}
