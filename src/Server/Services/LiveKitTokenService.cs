using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using SPQC.Confer.SelfHosted.Server.Configuration;
using SPQC.Confer.SelfHosted.Server.Models;
using SPQC.Confer.SelfHosted.Server.Telemetry;

namespace SPQC.Confer.SelfHosted.Server.Services;

public sealed class LiveKitTokenService : ILiveKitTokenService
{
    private readonly IChapterService _chapters;
    private readonly LiveKitSettings _settings;
    private readonly ILogger<LiveKitTokenService> _logger;
    private readonly IConferMetrics _metrics;

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public LiveKitTokenService(IChapterService chapters, LiveKitSettings settings,
        ILogger<LiveKitTokenService> logger, IConferMetrics metrics)
    {
        _chapters = chapters;
        _settings = settings;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<string> IssueRoomTokenAsync(Member member, Room room, Chapter chapter,
        CancellationToken ct = default)
    {
        var secret = await _chapters.GetDecryptedSecretAsync(chapter.Id, ct)
            ?? throw new InvalidOperationException($"Cannot decrypt secret for chapter {chapter.Id}");

        var grants = BuildGrants(member.Role, room.LiveKitRoomName);
        var grantsJson = JsonSerializer.Serialize(grants, _jsonOpts);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var now = DateTimeOffset.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, member.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("name", member.DisplayName),
            new("video", grantsJson, JsonClaimValueTypes.Json),
        };

        var token = new JwtSecurityToken(
            issuer: chapter.LiveKitApiKey,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(_settings.TokenExpiryMinutes).UtcDateTime,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _metrics.TokenIssued(member.Role.ToString());
        _logger.LogInformation("LiveKit token issued: member={MemberId} room={RoomName}", member.Id, room.LiveKitRoomName);
        return tokenString;
    }

    private static LiveKitGrants BuildGrants(MemberRole role, string roomName) => role switch
    {
        MemberRole.Host or MemberRole.ChapterAdmin or MemberRole.OrgAdmin => new LiveKitGrants(
            RoomJoin: true, Room: roomName,
            CanPublish: true, CanSubscribe: true, CanPublishData: true),

        MemberRole.Presenter => new LiveKitGrants(
            RoomJoin: true, Room: roomName,
            CanPublish: true, CanSubscribe: true, CanPublishData: false),

        _ => new LiveKitGrants(
            RoomJoin: true, Room: roomName,
            CanPublish: false, CanSubscribe: true, CanPublishData: false)
    };
}
