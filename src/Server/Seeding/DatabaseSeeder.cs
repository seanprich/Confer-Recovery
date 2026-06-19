using ConferRecovery.Server.Models;
using ConferRecovery.Server.Repositories;
using ConferRecovery.Server.Services;

namespace ConferRecovery.Server.Seeding;

public sealed class DatabaseSeeder
{
    private readonly IMemberService _members;
    private readonly IChapterService _chapters;
    private readonly IChapterRepository _chapterRepo;
    private readonly IMemberRepository _memberRepo;
    private readonly SeedSettings _settings;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        IMemberService members,
        IChapterService chapters,
        IChapterRepository chapterRepo,
        IMemberRepository memberRepo,
        SeedSettings settings,
        ILogger<DatabaseSeeder> logger)
    {
        _members = members;
        _chapters = chapters;
        _chapterRepo = chapterRepo;
        _memberRepo = memberRepo;
        _settings = settings;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogDebug("Seed:AdminEmail/AdminPassword not set — skipping seed.");
            return;
        }

        var existingAdmin = await _memberRepo.FindOneAsync(
            m => m.Role == MemberRole.OrgAdmin, ct);

        if (existingAdmin is not null)
        {
            _logger.LogInformation("OrgAdmin {Email} already exists — seed skipped.", existingAdmin.Email);
            return;
        }

        _logger.LogInformation("No OrgAdmin found — seeding initial data.");

        var chapter = await _chapters.CreateAsync(
            _settings.ChapterName,
            _settings.SfuUrl,
            _settings.LiveKitApiKey,
            _settings.LiveKitApiSecret,
            ct);

        var admin = await _members.CreateAsync(
            _settings.AdminDisplayName,
            _settings.AdminEmail!,
            _settings.AdminPassword!,
            chapter.Id,
            MemberRole.OrgAdmin,
            ct);

        await _members.UpdateStatusAsync(admin.Id, MemberStatus.Active, ct);

        chapter.AdminMemberIds.Add(admin.Id);
        await _chapterRepo.ReplaceAsync(chapter.Id, chapter, ct);

        _logger.LogInformation(
            "Seed complete — chapter {ChapterId}, OrgAdmin {MemberId} ({Email}).",
            chapter.Id, admin.Id, admin.Email);
    }
}
