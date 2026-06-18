using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SPQC.Confer.SelfHosted.Server.Models;
using SPQC.Confer.SelfHosted.Server.Repositories;
using SPQC.Confer.SelfHosted.Server.Services;
using SPQC.Confer.SelfHosted.Server.Telemetry;

namespace SPQC.Confer.SelfHosted.Tests.Services;

public sealed class RoomServiceTests
{
    private readonly IRoomRepository _rooms = Substitute.For<IRoomRepository>();
    private readonly IMemberRepository _members = Substitute.For<IMemberRepository>();
    private readonly IConferMetrics _metrics = Substitute.For<IConferMetrics>();
    private RoomService Sut() => new(_rooms, _members, NullLogger<RoomService>.Instance, _metrics);

    [Fact]
    public async Task CreateAsync_GeneratesUniqueLiveKitRoomName()
    {
        var room1 = await Sut().CreateAsync("chapId", "hostId", "Session 1");
        var room2 = await Sut().CreateAsync("chapId", "hostId", "Session 2");

        Assert.NotEqual(room1.LiveKitRoomName, room2.LiveKitRoomName);
    }

    [Fact]
    public async Task CreateAsync_RoomNameFitsWithin48Chars()
    {
        var room = await Sut().CreateAsync("507f1f77bcf86cd799439011", "hostId", "Test");
        Assert.True(room.LiveKitRoomName.Length <= 48);
    }

    [Fact]
    public async Task CreateAsync_SetsStatusToScheduled()
    {
        var room = await Sut().CreateAsync("chapId", "hostId", "Test");
        Assert.Equal(RoomStatus.Scheduled, room.Status);
    }

    [Fact]
    public async Task StartAsync_WithScheduledRoom_TransitionsToActive()
    {
        var room = new Room { Id = "roomId", Status = RoomStatus.Scheduled };
        _rooms.GetByIdAsync("roomId", default).Returns(room);
        _rooms.ReplaceAsync("roomId", Arg.Any<Room>(), default).Returns(true);

        var result = await Sut().StartAsync("roomId");

        Assert.NotNull(result);
        Assert.Equal(RoomStatus.Active, result!.Status);
        Assert.NotNull(result.StartedAt);
    }

    [Fact]
    public async Task StartAsync_WithActiveRoom_ReturnsNull()
    {
        var room = new Room { Id = "roomId", Status = RoomStatus.Active };
        _rooms.GetByIdAsync("roomId", default).Returns(room);

        var result = await Sut().StartAsync("roomId");

        Assert.Null(result);
    }

    [Fact]
    public async Task EndAsync_WithActiveRoom_TransitionsToEnded()
    {
        var room = new Room { Id = "roomId", Status = RoomStatus.Active };
        _rooms.GetByIdAsync("roomId", default).Returns(room);
        _rooms.ReplaceAsync("roomId", Arg.Any<Room>(), default).Returns(true);

        var result = await Sut().EndAsync("roomId");

        Assert.NotNull(result);
        Assert.Equal(RoomStatus.Ended, result!.Status);
        Assert.NotNull(result.EndedAt);
    }

    [Fact]
    public async Task CanMemberJoinAsync_WithActiveMemberInSameChapter_ReturnsTrue()
    {
        _rooms.GetByIdAsync("roomId", default).Returns(new Room
            { ChapterId = "chapId", Status = RoomStatus.Active });
        _members.GetByIdAsync("memberId", default).Returns(new Member
            { ChapterId = "chapId", Status = MemberStatus.Active });

        Assert.True(await Sut().CanMemberJoinAsync("roomId", "memberId"));
    }

    [Fact]
    public async Task CanMemberJoinAsync_WithDifferentChapter_ReturnsFalse()
    {
        _rooms.GetByIdAsync("roomId", default).Returns(new Room
            { ChapterId = "chapId-A", Status = RoomStatus.Active });
        _members.GetByIdAsync("memberId", default).Returns(new Member
            { ChapterId = "chapId-B", Status = MemberStatus.Active });

        Assert.False(await Sut().CanMemberJoinAsync("roomId", "memberId"));
    }

    [Fact]
    public async Task CanMemberJoinAsync_WithSuspendedMember_ReturnsFalse()
    {
        _rooms.GetByIdAsync("roomId", default).Returns(new Room
            { ChapterId = "chapId", Status = RoomStatus.Active });
        _members.GetByIdAsync("memberId", default).Returns(new Member
            { ChapterId = "chapId", Status = MemberStatus.Suspended });

        Assert.False(await Sut().CanMemberJoinAsync("roomId", "memberId"));
    }

    [Fact]
    public async Task CanMemberJoinAsync_WithEndedRoom_ReturnsFalse()
    {
        _rooms.GetByIdAsync("roomId", default).Returns(new Room
            { ChapterId = "chapId", Status = RoomStatus.Ended });

        Assert.False(await Sut().CanMemberJoinAsync("roomId", "memberId"));
    }
}
