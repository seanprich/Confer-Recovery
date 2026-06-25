using ConferRecovery.Desktop.Infrastructure.Generated;
using ConferRecovery.Desktop.Infrastructure.Rooms;
using NSubstitute;

namespace ConferRecovery.Tests.Desktop.Infrastructure;

public sealed class ApiRoomsClientTests
{
    [Fact]
    public async Task GetByChapterAsync_MapsGeneratedCollection()
    {
        var generated = Substitute.For<IRoomsClient>();
        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.AddHours(3));

        generated.RoomsGetByChapterAsync("chapter-1", Arg.Any<CancellationToken>())
            .Returns(new List<RoomResponse>
            {
                new()
                {
                    Id = "room-1",
                    ChapterId = "chapter-1",
                    Name = "Weekly",
                    HostMemberId = "host-1",
                    Status = "Scheduled",
                    ScheduledAt = scheduledAt,
                    StartedAt = null,
                    EndedAt = null,
                    LobbyEnabled = true,
                    MaxVideoPublishers = 4,
                    MaxParticipants = 50
                }
            });

        var sut = new ApiRoomsClient(generated);

        var result = await sut.GetByChapterAsync("chapter-1", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("room-1", result[0].Id);
        Assert.Equal(scheduledAt.UtcDateTime, result[0].ScheduledAt);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var generated = Substitute.For<IRoomsClient>();
        generated.RoomsGetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<RoomResponse>>(_ => throw new ConferApiException(
                "Not Found",
                404,
                string.Empty,
                new Dictionary<string, IEnumerable<string>>(),
                null!));

        var sut = new ApiRoomsClient(generated);

        var result = await sut.GetByIdAsync("missing", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_MapsRoom()
    {
        var generated = Substitute.For<IRoomsClient>();
        var startedAt = new DateTimeOffset(DateTime.UtcNow);

        generated.RoomsGetByIdAsync("room-1", Arg.Any<CancellationToken>())
            .Returns(new RoomResponse
            {
                Id = "room-1",
                ChapterId = "chapter-1",
                Name = "Weekly",
                HostMemberId = "host-1",
                Status = "Active",
                ScheduledAt = null,
                StartedAt = startedAt,
                EndedAt = null,
                LobbyEnabled = true,
                MaxVideoPublishers = 4,
                MaxParticipants = 50
            });

        var sut = new ApiRoomsClient(generated);

        var result = await sut.GetByIdAsync("room-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("room-1", result!.Id);
        Assert.Equal(startedAt.UtcDateTime, result.StartedAt);
    }

    [Fact]
    public async Task CreateAsync_MapsResponseToContract()
    {
        var generated = Substitute.For<IRoomsClient>();
        var scheduledAt = new DateTimeOffset(DateTime.UtcNow.AddHours(1));

        generated.RoomsCreateAsync(Arg.Any<CreateRoomRequest>(), Arg.Any<CancellationToken>())
            .Returns(new RoomResponse
            {
                Id = "room-1",
                ChapterId = "chapter-1",
                Name = "Weekly",
                HostMemberId = "host-1",
                Status = "Scheduled",
                ScheduledAt = scheduledAt,
                StartedAt = null,
                EndedAt = null,
                LobbyEnabled = true,
                MaxVideoPublishers = 4,
                MaxParticipants = 50
            });

        var sut = new ApiRoomsClient(generated);

        var result = await sut.CreateAsync(
            new ConferRecovery.Desktop.Contracts.Rooms.CreateRoomRequest("Weekly", "chapter-1", DateTime.UtcNow),
            CancellationToken.None);

        Assert.Equal("room-1", result.Id);
        Assert.Equal("Scheduled", result.Status);
    }

    [Fact]
    public async Task CreateAsync_WithNullScheduledAt_MapsResponseToContract()
    {
        var generated = Substitute.For<IRoomsClient>();

        generated.RoomsCreateAsync(Arg.Any<CreateRoomRequest>(), Arg.Any<CancellationToken>())
            .Returns(new RoomResponse
            {
                Id = "room-2",
                ChapterId = "chapter-1",
                Name = "Adhoc",
                HostMemberId = "host-1",
                Status = "Scheduled",
                ScheduledAt = null,
                StartedAt = null,
                EndedAt = null,
                LobbyEnabled = true,
                MaxVideoPublishers = 2,
                MaxParticipants = 20
            });

        var sut = new ApiRoomsClient(generated);

        var result = await sut.CreateAsync(
            new ConferRecovery.Desktop.Contracts.Rooms.CreateRoomRequest("Adhoc", "chapter-1", null),
            CancellationToken.None);

        Assert.Equal("room-2", result.Id);
        Assert.Null(result.ScheduledAt);
    }

    [Fact]
    public async Task StartAsync_MapsRoomResponse()
    {
        var generated = Substitute.For<IRoomsClient>();
        var startedAt = new DateTimeOffset(DateTime.UtcNow);

        generated.RoomsStartAsync("room-1", Arg.Any<CancellationToken>())
            .Returns(new RoomResponse
            {
                Id = "room-1",
                ChapterId = "chapter-1",
                Name = "Weekly",
                HostMemberId = "host-1",
                Status = "Active",
                ScheduledAt = null,
                StartedAt = startedAt,
                EndedAt = null,
                LobbyEnabled = true,
                MaxVideoPublishers = 4,
                MaxParticipants = 50
            });

        var sut = new ApiRoomsClient(generated);

        var result = await sut.StartAsync("room-1", CancellationToken.None);

        Assert.Equal("Active", result.Status);
        Assert.Equal(startedAt.UtcDateTime, result.StartedAt);
    }

    [Fact]
    public async Task EndAsync_MapsRoomResponse()
    {
        var generated = Substitute.For<IRoomsClient>();
        var endedAt = new DateTimeOffset(DateTime.UtcNow);

        generated.RoomsEndAsync("room-1", Arg.Any<CancellationToken>())
            .Returns(new RoomResponse
            {
                Id = "room-1",
                ChapterId = "chapter-1",
                Name = "Weekly",
                HostMemberId = "host-1",
                Status = "Ended",
                ScheduledAt = null,
                StartedAt = null,
                EndedAt = endedAt,
                LobbyEnabled = true,
                MaxVideoPublishers = 4,
                MaxParticipants = 50
            });

        var sut = new ApiRoomsClient(generated);

        var result = await sut.EndAsync("room-1", CancellationToken.None);

        Assert.Equal("Ended", result.Status);
        Assert.Equal(endedAt.UtcDateTime, result.EndedAt);
    }

    [Fact]
    public async Task JoinAsync_MapsGeneratedResponseToContract()
    {
        var generated = Substitute.For<IRoomsClient>();
        var expiresAt = new DateTimeOffset(DateTime.UtcNow.AddHours(2));

        generated.RoomsJoinAsync("room-1", Arg.Any<CancellationToken>())
            .Returns(new JoinRoomResponse
            {
                LiveKitToken = "lk-token",
                SfuUrl = "wss://sfu.example.org",
                RoomName = "room-1",
                TokenExpiresAt = expiresAt
            });

        var sut = new ApiRoomsClient(generated);

        var result = await sut.JoinAsync("room-1", CancellationToken.None);

        Assert.Equal("lk-token", result.LiveKitToken);
        Assert.Equal("wss://sfu.example.org", result.SfuUrl);
        Assert.Equal("room-1", result.RoomName);
        Assert.Equal(expiresAt.UtcDateTime, result.TokenExpiresAt);
    }
}