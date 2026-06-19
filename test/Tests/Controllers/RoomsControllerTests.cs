using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using ConferRecovery.Server.Controllers;
using ConferRecovery.Server.Models;
using ConferRecovery.Server.Services;

namespace ConferRecovery.Tests.Controllers;

public sealed class RoomsControllerTests
{
    private const string ChapA = "507f1f77bcf86cd799439011";
    private const string ChapB = "507f1f77bcf86cd799439012";
    private const string HostId = "507f1f77bcf86cd799439021";
    private const string OtherId = "507f1f77bcf86cd799439022";
    private const string RoomId = "507f1f77bcf86cd799439031";

    private readonly IRoomService _rooms = Substitute.For<IRoomService>();
    private readonly IChapterService _chapters = Substitute.For<IChapterService>();
    private readonly IMemberService _members = Substitute.For<IMemberService>();
    private readonly ILiveKitTokenService _liveKit = Substitute.For<ILiveKitTokenService>();
    private readonly IAuditService _audit = Substitute.For<IAuditService>();
    private readonly IConfiguration _config = Substitute.For<IConfiguration>();

    private RoomsController Sut(string role, string memberId, string chapterId) =>
        new(_rooms, _chapters, _members, _liveKit, _audit, _config)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim("sub", memberId),
                        new Claim("chapter", chapterId),
                        new Claim(ClaimTypes.Role, role),
                    ], "test"))
                }
            }
        };

    private static Room ScheduledRoom(string hostId, string chapterId) => new()
    {
        Id = RoomId,
        ChapterId = chapterId,
        HostMemberId = hostId,
        Status = RoomStatus.Scheduled,
        Name = "Test",
        LiveKitRoomName = "lk-test"
    };

    private static Room ActiveRoom(string hostId, string chapterId) => new()
    {
        Id = RoomId,
        ChapterId = chapterId,
        HostMemberId = hostId,
        Status = RoomStatus.Active,
        Name = "Test",
        LiveKitRoomName = "lk-test"
    };

    // ── Start ownership ───────────────────────────────────────────────────────

    [Fact]
    public async Task Start_RoomOwner_ReturnsOk()
    {
        var room = ScheduledRoom(HostId, ChapA);
        var started = ActiveRoom(HostId, ChapA);
        _rooms.GetByIdAsync(RoomId, default).Returns(room);
        _rooms.StartAsync(RoomId, default).Returns(started);

        var result = await Sut("Host", HostId, ChapA).Start(RoomId, default);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Start_NonOwnerHost_ReturnsForbid()
    {
        var room = ScheduledRoom(OtherId, ChapA);
        _rooms.GetByIdAsync(RoomId, default).Returns(room);

        var result = await Sut("Host", HostId, ChapA).Start(RoomId, default);

        Assert.IsType<ForbidResult>(result.Result);
        await _rooms.DidNotReceive().StartAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Start_ChapterAdmin_SameChapter_ReturnsOk()
    {
        _rooms.GetByIdAsync(RoomId, default).Returns(ScheduledRoom(OtherId, ChapA));
        _rooms.StartAsync(RoomId, default).Returns(ActiveRoom(OtherId, ChapA));

        var result = await Sut("ChapterAdmin", HostId, ChapA).Start(RoomId, default);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Start_ChapterAdmin_DifferentChapter_ReturnsForbid()
    {
        var room = ScheduledRoom(OtherId, ChapB);
        _rooms.GetByIdAsync(RoomId, default).Returns(room);

        var result = await Sut("ChapterAdmin", HostId, ChapA).Start(RoomId, default);

        Assert.IsType<ForbidResult>(result.Result);
        await _rooms.DidNotReceive().StartAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Start_OrgAdmin_AnyChapter_ReturnsOk()
    {
        _rooms.GetByIdAsync(RoomId, default).Returns(ScheduledRoom(OtherId, ChapB));
        _rooms.StartAsync(RoomId, default).Returns(ActiveRoom(OtherId, ChapB));

        var result = await Sut("OrgAdmin", HostId, ChapA).Start(RoomId, default);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Start_NonExistentRoom_ReturnsNotFound()
    {
        _rooms.GetByIdAsync(RoomId, default).Returns((Room?)null);

        var result = await Sut("OrgAdmin", HostId, ChapA).Start(RoomId, default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ── End ownership ─────────────────────────────────────────────────────────

    [Fact]
    public async Task End_RoomOwner_ReturnsOk()
    {
        _rooms.GetByIdAsync(RoomId, default).Returns(ActiveRoom(HostId, ChapA));
        _rooms.EndAsync(RoomId, default).Returns(new Room
        {
            Id = RoomId, ChapterId = ChapA, HostMemberId = HostId,
            Status = RoomStatus.Ended, Name = "Test", LiveKitRoomName = "lk-test"
        });

        var result = await Sut("Host", HostId, ChapA).End(RoomId, default);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task End_NonOwnerHost_ReturnsForbid()
    {
        var room = ActiveRoom(OtherId, ChapA);
        _rooms.GetByIdAsync(RoomId, default).Returns(room);

        var result = await Sut("Host", HostId, ChapA).End(RoomId, default);

        Assert.IsType<ForbidResult>(result.Result);
        await _rooms.DidNotReceive().EndAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task End_ChapterAdmin_DifferentChapter_ReturnsForbid()
    {
        var room = ActiveRoom(OtherId, ChapB);
        _rooms.GetByIdAsync(RoomId, default).Returns(room);

        var result = await Sut("ChapterAdmin", HostId, ChapA).End(RoomId, default);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task End_OrgAdmin_AnyRoom_ReturnsOk()
    {
        _rooms.GetByIdAsync(RoomId, default).Returns(ActiveRoom(OtherId, ChapB));
        _rooms.EndAsync(RoomId, default).Returns(new Room
        {
            Id = RoomId, ChapterId = ChapB, HostMemberId = OtherId,
            Status = RoomStatus.Ended, Name = "Test", LiveKitRoomName = "lk-test"
        });

        var result = await Sut("OrgAdmin", HostId, ChapA).End(RoomId, default);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    // ── GetByChapter scope ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByChapter_ChapterAdmin_OwnChapter_ReturnsOk()
    {
        _rooms.GetByChapterAsync(ChapA, default).Returns((IReadOnlyList<Room>)[]);

        var result = await Sut("ChapterAdmin", HostId, ChapA).GetByChapter(ChapA, default);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByChapter_ChapterAdmin_OtherChapter_ReturnsForbid()
    {
        var result = await Sut("ChapterAdmin", HostId, ChapA).GetByChapter(ChapB, default);

        Assert.IsType<ForbidResult>(result.Result);
        await _rooms.DidNotReceive().GetByChapterAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByChapter_OrgAdmin_AnyChapter_ReturnsOk()
    {
        _rooms.GetByChapterAsync(ChapB, default).Returns((IReadOnlyList<Room>)[]);

        var result = await Sut("OrgAdmin", HostId, ChapA).GetByChapter(ChapB, default);

        Assert.IsType<OkObjectResult>(result.Result);
    }
}
