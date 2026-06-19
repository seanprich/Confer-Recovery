using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ConferRecovery.Server.Configuration;
using ConferRecovery.Server.Models;
using ConferRecovery.Server.Services;
using ConferRecovery.Server.Telemetry;

namespace ConferRecovery.Tests.Services;

public sealed class LiveKitTokenServiceTests
{
    private const string ApiKey = "test-api-key";
    private const string ApiSecret = "test-livekit-secret-at-least-32-chars-long";

    private readonly IChapterService _chapterService = Substitute.For<IChapterService>();
    private readonly IConferMetrics _metrics = Substitute.For<IConferMetrics>();
    private readonly LiveKitSettings _settings = new() { TokenExpiryMinutes = 30 };

    private LiveKitTokenService Sut() =>
        new(_chapterService, _settings, NullLogger<LiveKitTokenService>.Instance, _metrics);

    private static (Chapter chapter, Room room, Member member) Fixtures(MemberRole role)
    {
        var chapter = new Chapter { Id = "chapId", LiveKitApiKey = ApiKey };
        var room = new Room { LiveKitRoomName = "test-room-name", ChapterId = "chapId" };
        var member = new Member { Id = "memberId", DisplayName = "Test User", Role = role };
        return (chapter, room, member);
    }

    private async Task<JsonDocument> DecodeVideoGrant(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var videoClaim = jwt.Claims.First(c => c.Type == "video").Value;
        return JsonDocument.Parse(videoClaim);
    }

    [Fact]
    public async Task IssueRoomTokenAsync_HostGetsBothPublishAndSubscribe()
    {
        var (chapter, room, member) = Fixtures(MemberRole.Host);
        _chapterService.GetDecryptedSecretAsync("chapId", default).Returns(ApiSecret);

        var token = await Sut().IssueRoomTokenAsync(member, room, chapter);
        using var doc = await DecodeVideoGrant(token);

        Assert.True(doc.RootElement.GetProperty("roomJoin").GetBoolean());
        Assert.True(doc.RootElement.GetProperty("canPublish").GetBoolean());
        Assert.True(doc.RootElement.GetProperty("canSubscribe").GetBoolean());
    }

    [Fact]
    public async Task IssueRoomTokenAsync_ListenerGetsSubscribeOnly()
    {
        var (chapter, room, member) = Fixtures(MemberRole.Listener);
        _chapterService.GetDecryptedSecretAsync("chapId", default).Returns(ApiSecret);

        var token = await Sut().IssueRoomTokenAsync(member, room, chapter);
        using var doc = await DecodeVideoGrant(token);

        Assert.False(doc.RootElement.GetProperty("canPublish").GetBoolean());
        Assert.True(doc.RootElement.GetProperty("canSubscribe").GetBoolean());
    }

    [Fact]
    public async Task IssueRoomTokenAsync_PresenterCanPublishButNotPublishData()
    {
        var (chapter, room, member) = Fixtures(MemberRole.Presenter);
        _chapterService.GetDecryptedSecretAsync("chapId", default).Returns(ApiSecret);

        var token = await Sut().IssueRoomTokenAsync(member, room, chapter);
        using var doc = await DecodeVideoGrant(token);

        Assert.True(doc.RootElement.GetProperty("canPublish").GetBoolean());
        Assert.False(doc.RootElement.GetProperty("canPublishData").GetBoolean());
    }

    [Fact]
    public async Task IssueRoomTokenAsync_TokenContainsCorrectRoomName()
    {
        var (chapter, room, member) = Fixtures(MemberRole.Host);
        _chapterService.GetDecryptedSecretAsync("chapId", default).Returns(ApiSecret);

        var token = await Sut().IssueRoomTokenAsync(member, room, chapter);
        using var doc = await DecodeVideoGrant(token);

        Assert.Equal("test-room-name", doc.RootElement.GetProperty("room").GetString());
    }

    [Fact]
    public async Task IssueRoomTokenAsync_TokenIsSignedWithApiKey()
    {
        var (chapter, room, member) = Fixtures(MemberRole.Host);
        _chapterService.GetDecryptedSecretAsync("chapId", default).Returns(ApiSecret);

        var token = await Sut().IssueRoomTokenAsync(member, room, chapter);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(ApiKey, jwt.Issuer);
    }

    [Fact]
    public async Task IssueRoomTokenAsync_EachTokenHasUniqueJti()
    {
        var (chapter, room, member) = Fixtures(MemberRole.Host);
        _chapterService.GetDecryptedSecretAsync("chapId", default).Returns(ApiSecret);

        var sut = Sut();
        var token1 = await sut.IssueRoomTokenAsync(member, room, chapter);
        var token2 = await sut.IssueRoomTokenAsync(member, room, chapter);

        var jwt1 = new JwtSecurityTokenHandler().ReadJwtToken(token1);
        var jwt2 = new JwtSecurityTokenHandler().ReadJwtToken(token2);

        Assert.NotEqual(jwt1.Id, jwt2.Id);
    }
}
