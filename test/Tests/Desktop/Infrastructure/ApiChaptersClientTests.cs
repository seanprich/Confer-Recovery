using ConferRecovery.Desktop.Infrastructure.Chapters;
using ConferRecovery.Desktop.Infrastructure.Generated;
using NSubstitute;

namespace ConferRecovery.Tests.Desktop.Infrastructure;

public sealed class ApiChaptersClientTests
{
    [Fact]
    public async Task GetActiveAsync_MapsGeneratedCollection()
    {
        var generated = Substitute.For<IChaptersClient>();
        var createdAt = new DateTimeOffset(DateTime.UtcNow.AddDays(-2));

        generated.ChaptersGetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ChapterResponse>
            {
                new()
                {
                    Id = "chapter-1",
                    Name = "Main",
                    SfuUrl = "wss://sfu.example.org",
                    LiveKitApiKey = "lk-key",
                    Status = "Active",
                    CreatedAt = createdAt
                }
            });

        var sut = new ApiChaptersClient(generated);

        var result = await sut.GetActiveAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("chapter-1", result[0].Id);
        Assert.Equal(createdAt.UtcDateTime, result[0].CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var generated = Substitute.For<IChaptersClient>();
        generated.ChaptersGetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<ChapterResponse>>(_ => throw new ConferApiException(
                "Not Found",
                404,
                string.Empty,
                new Dictionary<string, IEnumerable<string>>(),
                null!));

        var sut = new ApiChaptersClient(generated);

        var result = await sut.GetByIdAsync("missing", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_MapsResponse()
    {
        var generated = Substitute.For<IChaptersClient>();
        var createdAt = new DateTimeOffset(DateTime.UtcNow.AddDays(-5));

        generated.ChaptersGetByIdAsync("chapter-1", Arg.Any<CancellationToken>())
            .Returns(new ChapterResponse
            {
                Id = "chapter-1",
                Name = "Main",
                SfuUrl = "wss://sfu.example.org",
                LiveKitApiKey = "lk-key",
                Status = "Active",
                CreatedAt = createdAt
            });

        var sut = new ApiChaptersClient(generated);

        var result = await sut.GetByIdAsync("chapter-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("chapter-1", result!.Id);
        Assert.Equal(createdAt.UtcDateTime, result.CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_MapsResponse()
    {
        var generated = Substitute.For<IChaptersClient>();
        var createdAt = new DateTimeOffset(DateTime.UtcNow.AddDays(-3));

        generated.ChaptersCreateAsync(Arg.Any<CreateChapterRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ChapterResponse
            {
                Id = "chapter-1",
                Name = "Main",
                SfuUrl = "wss://sfu.example.org",
                LiveKitApiKey = "lk-key",
                Status = "Active",
                CreatedAt = createdAt
            });

        var sut = new ApiChaptersClient(generated);

        var result = await sut.CreateAsync(
            new ConferRecovery.Desktop.Contracts.Chapters.CreateChapterRequest("Main", "wss://sfu", "k", "s"),
            CancellationToken.None);

        Assert.Equal("chapter-1", result.Id);
        Assert.Equal(createdAt.UtcDateTime, result.CreatedAt);
    }

    [Fact]
    public async Task UpdateSfuAsync_WhenSuccessful_ReturnsTrue()
    {
        var generated = Substitute.For<IChaptersClient>();
        var sut = new ApiChaptersClient(generated);

        var success = await sut.UpdateSfuAsync(
            "chapter-1",
            new ConferRecovery.Desktop.Contracts.Chapters.CreateChapterRequest("Main", "wss://sfu", "k", "s"),
            CancellationToken.None);

        Assert.True(success);
        await generated.Received(1).ChaptersUpdateSfuAsync(
            "chapter-1",
            Arg.Is<CreateChapterRequest>(x => x.SfuUrl == "wss://sfu" && x.LiveKitApiKey == "k"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSfuAsync_WhenNotFound_ReturnsFalse()
    {
        var generated = Substitute.For<IChaptersClient>();
        generated.ChaptersUpdateSfuAsync(Arg.Any<string>(), Arg.Any<CreateChapterRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConferApiException(
                "Not Found",
                404,
                string.Empty,
                new Dictionary<string, IEnumerable<string>>(),
                null!));

        var sut = new ApiChaptersClient(generated);

        var success = await sut.UpdateSfuAsync(
            "chapter-1",
            new ConferRecovery.Desktop.Contracts.Chapters.CreateChapterRequest("Main", "wss://sfu", "k", "s"),
            CancellationToken.None);

        Assert.False(success);
    }

    [Fact]
    public async Task SetStatusAsync_WhenSuccessful_ReturnsTrue()
    {
        var generated = Substitute.For<IChaptersClient>();
        var sut = new ApiChaptersClient(generated);

        var success = await sut.SetStatusAsync("chapter-1", "Inactive", CancellationToken.None);

        Assert.True(success);
        await generated.Received(1).ChaptersSetStatusAsync("chapter-1", "Inactive", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetStatusAsync_WhenNotFound_ReturnsFalse()
    {
        var generated = Substitute.For<IChaptersClient>();
        generated.ChaptersSetStatusAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConferApiException(
                "Not Found",
                404,
                string.Empty,
                new Dictionary<string, IEnumerable<string>>(),
                null!));

        var sut = new ApiChaptersClient(generated);

        var success = await sut.SetStatusAsync("chapter-1", "Inactive", CancellationToken.None);

        Assert.False(success);
    }
}