using MongoDB.Bson;
using SPQC.Confer.SelfHosted.Server.Models;
using SPQC.Confer.SelfHosted.Server.Repositories;
using SPQC.Confer.SelfHosted.Tests.Fixtures;

namespace SPQC.Confer.SelfHosted.Tests.Repositories;

public sealed class RoomRepositoryTests : IAsyncLifetime
{
    private readonly MongoFixture _mongo = new();
    private RoomRepository Repo() => new(_mongo.Database);

    public Task InitializeAsync() => _mongo.InitializeAsync();
    public Task DisposeAsync() => _mongo.DisposeAsync();

    private static string NewId() => ObjectId.GenerateNewId().ToString();

    private static Room NewRoom(string? chapterId = null, RoomStatus status = RoomStatus.Scheduled) =>
        new()
        {
            ChapterId = chapterId ?? NewId(),
            Name = "Weekly Meeting",
            LiveKitRoomName = Guid.NewGuid().ToString("N"),
            HostMemberId = NewId(),
            Status = status
        };

    [Fact]
    public async Task InsertAndGetByLiveKitRoomName_RoundTrips()
    {
        var repo = Repo();
        var room = NewRoom();
        await repo.InsertAsync(room);

        var result = await repo.GetByLiveKitRoomNameAsync(room.LiveKitRoomName);

        Assert.NotNull(result);
        Assert.Equal(room.LiveKitRoomName, result!.LiveKitRoomName);
    }

    [Fact]
    public async Task GetActiveRoomForChapterAsync_ReturnsActiveRoom()
    {
        var repo = Repo();
        var chapId = NewId();
        await repo.InsertAsync(NewRoom(chapId, RoomStatus.Ended));
        var active = NewRoom(chapId, RoomStatus.Active);
        await repo.InsertAsync(active);

        var result = await repo.GetActiveRoomForChapterAsync(chapId);

        Assert.NotNull(result);
        Assert.Equal(active.LiveKitRoomName, result!.LiveKitRoomName);
    }

    [Fact]
    public async Task GetActiveRoomForChapterAsync_WithNoActiveRoom_ReturnsNull()
    {
        var repo = Repo();
        var chapId = NewId();
        await repo.InsertAsync(NewRoom(chapId, RoomStatus.Ended));

        var result = await repo.GetActiveRoomForChapterAsync(chapId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByChapterAsync_ReturnsAllRoomsForChapter()
    {
        var repo = Repo();
        var chapR = NewId();
        await repo.InsertAsync(NewRoom(chapR));
        await repo.InsertAsync(NewRoom(chapR, RoomStatus.Active));
        await repo.InsertAsync(NewRoom());  // different chapter

        var results = await repo.GetByChapterAsync(chapR);

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Equal(chapR, r.ChapterId));
    }

    [Fact]
    public async Task ReplaceAsync_UpdatesRoomStatus()
    {
        var repo = Repo();
        var room = NewRoom();
        await repo.InsertAsync(room);

        room.Status = RoomStatus.Active;
        room.StartedAt = DateTime.UtcNow;
        await repo.ReplaceAsync(room.Id, room);

        var result = await repo.GetByIdAsync(room.Id);
        Assert.Equal(RoomStatus.Active, result!.Status);
        Assert.NotNull(result.StartedAt);
    }
}
