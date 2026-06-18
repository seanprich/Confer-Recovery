using Microsoft.AspNetCore.DataProtection;
using ConferRecovery.Server.Models;
using ConferRecovery.Server.Repositories;

namespace ConferRecovery.Server.Services;

public sealed class ChapterService : IChapterService
{
    private readonly IChapterRepository _chapters;
    private readonly IDataProtector _protector;
    private readonly ILogger<ChapterService> _logger;

    public ChapterService(IChapterRepository chapters, IDataProtectionProvider dpProvider,
        ILogger<ChapterService> logger)
    {
        _chapters = chapters;
        _protector = dpProvider.CreateProtector("LiveKitApiSecrets");
        _logger = logger;
    }

    public Task<Chapter?> GetByIdAsync(string id, CancellationToken ct = default)
        => _chapters.GetByIdAsync(id, ct);

    public Task<IReadOnlyList<Chapter>> GetAllActiveAsync(CancellationToken ct = default)
        => _chapters.GetActiveAsync(ct);

    public async Task<Chapter> CreateAsync(string name, string sfuUrl, string liveKitApiKey,
        string liveKitApiSecret, CancellationToken ct = default)
    {
        var chapter = new Chapter
        {
            Name = name,
            SfuUrl = sfuUrl,
            LiveKitApiKey = liveKitApiKey,
            LiveKitApiSecretEncrypted = _protector.Protect(liveKitApiSecret)
        };

        await _chapters.InsertAsync(chapter, ct);
        _logger.LogInformation("Chapter created: {ChapterId} ({Name})", chapter.Id, name);
        return chapter;
    }

    public async Task<bool> UpdateSfuAsync(string id, string sfuUrl, string liveKitApiKey,
        string liveKitApiSecret, CancellationToken ct = default)
    {
        var chapter = await _chapters.GetByIdAsync(id, ct);
        if (chapter is null) return false;
        chapter.SfuUrl = sfuUrl;
        chapter.LiveKitApiKey = liveKitApiKey;
        chapter.LiveKitApiSecretEncrypted = _protector.Protect(liveKitApiSecret);
        return await _chapters.ReplaceAsync(id, chapter, ct);
    }

    public async Task<bool> SetStatusAsync(string id, ChapterStatus status, CancellationToken ct = default)
    {
        var chapter = await _chapters.GetByIdAsync(id, ct);
        if (chapter is null) return false;
        chapter.Status = status;
        return await _chapters.ReplaceAsync(id, chapter, ct);
    }

    public async Task<string?> GetDecryptedSecretAsync(string chapterId, CancellationToken ct = default)
    {
        var chapter = await _chapters.GetByIdAsync(chapterId, ct);
        if (chapter is null) return null;
        return _protector.Unprotect(chapter.LiveKitApiSecretEncrypted);
    }
}
