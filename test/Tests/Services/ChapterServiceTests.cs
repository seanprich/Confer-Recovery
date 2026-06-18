using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ConferRecovery.Server.Models;
using ConferRecovery.Server.Repositories;
using ConferRecovery.Server.Services;

namespace ConferRecovery.Tests.Services;

public sealed class ChapterServiceTests
{
    private readonly IChapterRepository _repo = Substitute.For<IChapterRepository>();

    // Use real DataProtection so we can verify encrypt/decrypt round-trips
    private readonly IDataProtectionProvider _dpProvider =
        DataProtectionProvider.Create("spqc-confer-test");

    private ChapterService Sut() =>
        new(_repo, _dpProvider, NullLogger<ChapterService>.Instance);

    [Fact]
    public async Task CreateAsync_DoesNotStoreSecretInPlaintext()
    {
        const string secret = "super-secret-livekit-api-key";
        Chapter? captured = null;
        await _repo.InsertAsync(Arg.Do<Chapter>(c => captured = c), default);

        await Sut().CreateAsync("Chapter A", "wss://sfu.example.org", "api-key", secret);

        Assert.NotNull(captured);
        Assert.NotEqual(secret, captured.LiveKitApiSecretEncrypted);
    }

    [Fact]
    public async Task GetDecryptedSecretAsync_ReturnsOriginalSecret()
    {
        const string secret = "super-secret-livekit-api-key";
        Chapter? stored = null;
        await _repo.InsertAsync(Arg.Do<Chapter>(c => stored = c), default);
        await Sut().CreateAsync("Chapter A", "wss://sfu.example.org", "api-key", secret);

        _repo.GetByIdAsync(Arg.Any<string>(), default).Returns(stored!);

        var decrypted = await Sut().GetDecryptedSecretAsync("anyId");

        Assert.Equal(secret, decrypted);
    }

    [Fact]
    public async Task CreateAsync_SetsActiveStatus()
    {
        Chapter? captured = null;
        await _repo.InsertAsync(Arg.Do<Chapter>(c => captured = c), default);

        await Sut().CreateAsync("Chapter B", "wss://sfu.example.org", "api-key", "secret");

        Assert.Equal(ChapterStatus.Active, captured!.Status);
    }

    [Fact]
    public async Task UpdateSfuAsync_ReEncryptsNewSecret()
    {
        const string original = "original-secret";
        const string updated = "new-secret";

        var chapter = new Chapter
        {
            Id = "chapId",
            LiveKitApiSecretEncrypted = _dpProvider
                .CreateProtector("LiveKitApiSecrets").Protect(original)
        };
        _repo.GetByIdAsync("chapId", default).Returns(chapter);
        _repo.ReplaceAsync("chapId", Arg.Any<Chapter>(), default).Returns(true);

        Chapter? saved = null;
        _repo.ReplaceAsync("chapId", Arg.Do<Chapter>(c => saved = c), default).Returns(true);

        await Sut().UpdateSfuAsync("chapId", "wss://new-sfu.example.org", "new-key", updated);

        Assert.NotNull(saved);
        Assert.NotEqual(updated, saved!.LiveKitApiSecretEncrypted);

        // Verify it decrypts back correctly
        var protector = _dpProvider.CreateProtector("LiveKitApiSecrets");
        Assert.Equal(updated, protector.Unprotect(saved.LiveKitApiSecretEncrypted));
    }
}
