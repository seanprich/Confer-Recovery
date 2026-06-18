using MongoDB.Bson;
using MongoDB.Driver;
using ConferRecovery.Server.Models;
using ConferRecovery.Server.Repositories;
using ConferRecovery.Tests.Fixtures;

namespace ConferRecovery.Tests.Repositories;

public sealed class MemberRepositoryTests : IAsyncLifetime
{
    private readonly MongoFixture _mongo = new();
    private MemberRepository Repo() => new(_mongo.Database);

    public Task InitializeAsync() => _mongo.InitializeAsync();
    public Task DisposeAsync() => _mongo.DisposeAsync();

    private static string NewId() => ObjectId.GenerateNewId().ToString();

    // Fixed chapter ObjectIds shared across tests in this class
    private static readonly string ChapA = ObjectId.GenerateNewId().ToString();
    private static readonly string ChapB = ObjectId.GenerateNewId().ToString();

    private static Member NewMember(string email = "alice@example.com", string? chapterId = null) =>
        new()
        {
            DisplayName = "Alice",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass"),
            ChapterId = chapterId ?? ChapA,
            Role = MemberRole.Listener,
            Status = MemberStatus.Active
        };

    [Fact]
    public async Task InsertAndGetByEmail_RoundTrips()
    {
        var repo = Repo();
        await repo.InsertAsync(NewMember("bob@example.com"));

        var result = await repo.GetByEmailAsync("bob@example.com");

        Assert.NotNull(result);
        Assert.Equal("bob@example.com", result!.Email);
    }

    [Fact]
    public async Task InsertAndGetById_RoundTrips()
    {
        var repo = Repo();
        var member = NewMember("carol@example.com");
        await repo.InsertAsync(member);

        var result = await repo.GetByIdAsync(member.Id);

        Assert.NotNull(result);
        Assert.Equal(member.Id, result!.Id);
    }

    [Fact]
    public async Task GetByChapterAsync_ReturnsOnlyMatchingMembers()
    {
        var repo = Repo();
        await repo.InsertAsync(NewMember("d@x.com", ChapA));
        await repo.InsertAsync(NewMember("e@x.com", ChapA));
        await repo.InsertAsync(NewMember("f@x.com", ChapB));

        var chapAMembers = await repo.GetByChapterAsync(ChapA);

        Assert.Equal(2, chapAMembers.Count);
        Assert.All(chapAMembers, m => Assert.Equal(ChapA, m.ChapterId));
    }

    [Fact]
    public async Task ReplaceAsync_UpdatesStoredMember()
    {
        var repo = Repo();
        var member = NewMember("g@x.com");
        await repo.InsertAsync(member);

        member.Status = MemberStatus.Suspended;
        await repo.ReplaceAsync(member.Id, member);

        var result = await repo.GetByIdAsync(member.Id);
        Assert.Equal(MemberStatus.Suspended, result!.Status);
    }

    [Fact]
    public async Task DeleteAsync_RemovesMember()
    {
        var repo = Repo();
        var member = NewMember("h@x.com");
        await repo.InsertAsync(member);

        var deleted = await repo.DeleteAsync(member.Id);
        var result = await repo.GetByIdAsync(member.Id);

        Assert.True(deleted);
        Assert.Null(result);
    }

    [Fact]
    public async Task InsertDuplicateEmail_ThrowsMongoWriteException()
    {
        var repo = Repo();
        await repo.InsertAsync(NewMember("dup@x.com"));

        await Assert.ThrowsAnyAsync<MongoWriteException>(() =>
            repo.InsertAsync(NewMember("dup@x.com")));
    }
}
