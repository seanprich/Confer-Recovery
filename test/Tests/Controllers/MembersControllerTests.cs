using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using ConferRecovery.Server.Controllers;
using ConferRecovery.Server.DTOs.Members;
using ConferRecovery.Server.Models;
using ConferRecovery.Server.Services;

namespace ConferRecovery.Tests.Controllers;

public sealed class MembersControllerTests
{
    private const string ChapA = "507f1f77bcf86cd799439011";
    private const string ChapB = "507f1f77bcf86cd799439012";
    private const string MemberIdA = "507f1f77bcf86cd799439021";

    private readonly IMemberService _members = Substitute.For<IMemberService>();

    private MembersController Sut(string role, string chapterId) =>
        new(_members)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim("sub", MemberIdA),
                        new Claim("chapter", chapterId),
                        new Claim(ClaimTypes.Role, role),
                    ], "test"))
                }
            }
        };

    // ── GetByChapter ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByChapter_ChapterAdmin_OwnChapter_ReturnsOk()
    {
        _members.GetByChapterAsync(ChapA, default).Returns(
            (IReadOnlyList<Member>)[new Member { Id = MemberIdA, ChapterId = ChapA }]);

        var result = await Sut("ChapterAdmin", ChapA).GetByChapter(ChapA, default);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByChapter_ChapterAdmin_OtherChapter_ReturnsForbid()
    {
        var result = await Sut("ChapterAdmin", ChapA).GetByChapter(ChapB, default);

        Assert.IsType<ForbidResult>(result.Result);
        await _members.DidNotReceive().GetByChapterAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByChapter_OrgAdmin_AnyChapter_ReturnsOk()
    {
        _members.GetByChapterAsync(ChapB, default).Returns((IReadOnlyList<Member>)[]);

        var result = await Sut("OrgAdmin", ChapA).GetByChapter(ChapB, default);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ChapterAdmin_OwnChapter_ReturnsCreated()
    {
        var newMember = new Member { Id = MemberIdA, ChapterId = ChapA };
        _members.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            ChapA, Arg.Any<MemberRole>(), Arg.Any<CancellationToken>()).Returns(newMember);

        var request = new CreateMemberRequest("Bob", "bob@x.com", "password123456", ChapA);
        var result = await Sut("ChapterAdmin", ChapA).Create(request, default);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Create_ChapterAdmin_OtherChapter_ReturnsForbid()
    {
        var request = new CreateMemberRequest("Bob", "bob@x.com", "password123456", ChapB);
        var result = await Sut("ChapterAdmin", ChapA).Create(request, default);

        Assert.IsType<ForbidResult>(result.Result);
        await _members.DidNotReceive().CreateAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<MemberRole>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_OrgAdmin_AnyChapter_ReturnsCreated()
    {
        var newMember = new Member { Id = MemberIdA, ChapterId = ChapB };
        _members.CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            ChapB, Arg.Any<MemberRole>(), Arg.Any<CancellationToken>()).Returns(newMember);

        var request = new CreateMemberRequest("Bob", "bob@x.com", "password123456", ChapB);
        var result = await Sut("OrgAdmin", ChapA).Create(request, default);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    // ── UpdateStatus ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatus_ChapterAdmin_MemberInOwnChapter_ReturnsNoContent()
    {
        var target = new Member { Id = MemberIdA, ChapterId = ChapA };
        _members.GetByIdAsync(MemberIdA, default).Returns(target);
        _members.UpdateStatusAsync(MemberIdA, MemberStatus.Active, default).Returns(true);

        var result = await Sut("ChapterAdmin", ChapA)
            .UpdateStatus(MemberIdA, new UpdateMemberStatusRequest("Active"), default);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateStatus_ChapterAdmin_MemberInOtherChapter_ReturnsForbid()
    {
        var target = new Member { Id = MemberIdA, ChapterId = ChapB };
        _members.GetByIdAsync(MemberIdA, default).Returns(target);

        var result = await Sut("ChapterAdmin", ChapA)
            .UpdateStatus(MemberIdA, new UpdateMemberStatusRequest("Active"), default);

        Assert.IsType<ForbidResult>(result);
        await _members.DidNotReceive().UpdateStatusAsync(
            Arg.Any<string>(), Arg.Any<MemberStatus>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateStatus_OrgAdmin_MemberInAnyChapter_ReturnsNoContent()
    {
        _members.UpdateStatusAsync(MemberIdA, MemberStatus.Suspended, default).Returns(true);

        var result = await Sut("OrgAdmin", ChapA)
            .UpdateStatus(MemberIdA, new UpdateMemberStatusRequest("Suspended"), default);

        Assert.IsType<NoContentResult>(result);
        // OrgAdmin skips the pre-fetch
        await _members.DidNotReceive().GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
