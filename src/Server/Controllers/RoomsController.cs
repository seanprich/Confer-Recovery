using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConferRecovery.Server.DTOs.Rooms;
using ConferRecovery.Server.Extensions;
using ConferRecovery.Server.Models;
using ConferRecovery.Server.Services;

namespace ConferRecovery.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class RoomsController : ControllerBase
{
    private readonly IRoomService _rooms;
    private readonly IChapterService _chapters;
    private readonly IMemberService _members;
    private readonly ILiveKitTokenService _liveKitTokens;
    private readonly IAuditService _audit;
    private readonly IConfiguration _settings;

    public RoomsController(IRoomService rooms, IChapterService chapters, IMemberService members,
        ILiveKitTokenService liveKitTokens, IAuditService audit, IConfiguration settings)
    {
        _rooms = rooms;
        _chapters = chapters;
        _members = members;
        _liveKitTokens = liveKitTokens;
        _audit = audit;
        _settings = settings;
    }

    [HttpGet(Name = "RoomsGetByChapter")]
    public async Task<ActionResult<IReadOnlyList<RoomResponse>>> GetByChapter(
        [FromQuery] string chapterId, CancellationToken ct)
    {
        // ChapterAdmin may only query their own chapter; OrgAdmin unrestricted
        if (User.IsChapterAdmin() && !User.IsOrgAdmin() && User.ChapterId() != chapterId)
            return Forbid();

        var rooms = await _rooms.GetByChapterAsync(chapterId, ct);
        return Ok(rooms.Select(ToResponse).ToList());
    }

    [HttpGet("{id}", Name = "RoomsGetById")]
    public async Task<ActionResult<RoomResponse>> GetById(string id, CancellationToken ct)
    {
        var room = await _rooms.GetByIdAsync(id, ct);
        return room is null ? NotFound() : Ok(ToResponse(room));
    }

    [HttpPost(Name = "RoomsCreate")]
    [Authorize(Roles = "Host,ChapterAdmin,OrgAdmin")]
    public async Task<ActionResult<RoomResponse>> Create(
        [FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        // ChapterAdmin and Host may only create rooms in their own chapter
        if (!User.IsOrgAdmin() && User.ChapterId() != request.ChapterId)
            return Forbid();

        var hostId = User.MemberId();
        if (hostId is null) return Unauthorized();

        var room = await _rooms.CreateAsync(request.ChapterId, hostId, request.Name, request.ScheduledAt, ct);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, ToResponse(room));
    }

    [HttpPost("{id}/start", Name = "RoomsStart")]
    [Authorize(Roles = "Host,ChapterAdmin,OrgAdmin")]
    public async Task<ActionResult<RoomResponse>> Start(string id, CancellationToken ct)
    {
        var room = await _rooms.GetByIdAsync(id, ct);
        if (room is null) return NotFound();

        if (!IsAuthorizedForRoom(room)) return Forbid();

        var updated = await _rooms.StartAsync(id, ct);
        return updated is null ? Conflict(new { error = "Room is not in Scheduled state." }) : Ok(ToResponse(updated));
    }

    [HttpPost("{id}/end", Name = "RoomsEnd")]
    [Authorize(Roles = "Host,ChapterAdmin,OrgAdmin")]
    public async Task<ActionResult<RoomResponse>> End(string id, CancellationToken ct)
    {
        var room = await _rooms.GetByIdAsync(id, ct);
        if (room is null) return NotFound();

        if (!IsAuthorizedForRoom(room)) return Forbid();

        var updated = await _rooms.EndAsync(id, ct);
        return updated is null ? Conflict(new { error = "Room is not in Active state." }) : Ok(ToResponse(updated));
    }

    /// <summary>
    /// Issues a short-lived, room-scoped LiveKit token for the authenticated member.
    /// The caller must have acknowledged consent before receiving a token.
    /// </summary>
    [HttpPost("{id}/join", Name = "RoomsJoin")]
    public async Task<ActionResult<JoinRoomResponse>> Join(string id, CancellationToken ct)
    {
        var memberId = User.MemberId();
        if (memberId is null) return Unauthorized();

        var member = await _members.GetByIdAsync(memberId, ct);
        if (member is null) return Unauthorized();

        if (!member.ConsentAcknowledgedAt.HasValue)
            return Forbid();

        var canJoin = await _rooms.CanMemberJoinAsync(id, memberId, ct);
        if (!canJoin) return Forbid();

        var room = (await _rooms.GetByIdAsync(id, ct))!;
        var chapter = await _chapters.GetByIdAsync(room.ChapterId, ct);
        if (chapter is null) return Problem("Chapter configuration unavailable.", statusCode: 500);

        var lkToken = await _liveKitTokens.IssueRoomTokenAsync(member, room, chapter, ct);
        var expiryMinutes = _settings.GetValue<int>("LiveKit:TokenExpiryMinutes", 120);

        await _audit.RecordAsync(room.Id, member.Id, member.DisplayName,
            AuditEventType.TokenIssued, null, ct);

        return Ok(new JoinRoomResponse(
            LiveKitToken: lkToken,
            SfuUrl: chapter.SfuUrl,
            RoomName: room.LiveKitRoomName,
            TokenExpiresAt: DateTime.UtcNow.AddMinutes(expiryMinutes)));
    }

    // OrgAdmin: unrestricted.
    // ChapterAdmin: only rooms in their chapter.
    // Host: only rooms they own.
    private bool IsAuthorizedForRoom(Room room)
    {
        if (User.IsOrgAdmin()) return true;
        if (User.IsChapterAdmin()) return room.ChapterId == User.ChapterId();
        return room.HostMemberId == User.MemberId();
    }

    private static RoomResponse ToResponse(Room r) => new(
        r.Id, r.ChapterId, r.Name, r.HostMemberId,
        r.Status.ToString(), r.ScheduledAt, r.StartedAt, r.EndedAt,
        r.LobbyEnabled, r.MaxVideoPublishers, r.MaxParticipants);
}
