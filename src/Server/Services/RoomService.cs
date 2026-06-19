using ConferRecovery.Server.Models;
using ConferRecovery.Server.Repositories;
using ConferRecovery.Server.Telemetry;

namespace ConferRecovery.Server.Services;

public sealed class RoomService : IRoomService
{
    private readonly IRoomRepository _rooms;
    private readonly IMemberRepository _members;
    private readonly ILogger<RoomService> _logger;
    private readonly IConferMetrics _metrics;

    public RoomService(IRoomRepository rooms, IMemberRepository members,
        ILogger<RoomService> logger, IConferMetrics metrics)
    {
        _rooms = rooms;
        _members = members;
        _logger = logger;
        _metrics = metrics;
    }

    public Task<Room?> GetByIdAsync(string id, CancellationToken ct = default)
        => _rooms.GetByIdAsync(id, ct);

    public Task<IReadOnlyList<Room>> GetByChapterAsync(string chapterId, CancellationToken ct = default)
        => _rooms.GetByChapterAsync(chapterId, ct);

    public async Task<Room> CreateAsync(string chapterId, string hostMemberId, string name,
        DateTime? scheduledAt = null, CancellationToken ct = default)
    {
        // Collision-resistant room name: chapter prefix + date + GUID, capped at 48 chars
        var prefix = chapterId.Length >= 8 ? chapterId[..8] : chapterId;
        var lkRoomName = $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..48];

        var room = new Room
        {
            ChapterId = chapterId,
            HostMemberId = hostMemberId,
            Name = name,
            LiveKitRoomName = lkRoomName,
            ScheduledAt = scheduledAt,
            Status = RoomStatus.Scheduled
        };

        await _rooms.InsertAsync(room, ct);
        _metrics.RoomCreated();
        _logger.LogInformation("Room created: {RoomId} for chapter {ChapterId}", room.Id, chapterId);
        return room;
    }

    public async Task<Room?> StartAsync(string roomId, CancellationToken ct = default)
    {
        var room = await _rooms.GetByIdAsync(roomId, ct);
        if (room is null || room.Status != RoomStatus.Scheduled) return null;

        room.Status = RoomStatus.Active;
        room.StartedAt = DateTime.UtcNow;
        await _rooms.ReplaceAsync(roomId, room, ct);
        _metrics.RoomStarted();
        return room;
    }

    public async Task<Room?> EndAsync(string roomId, CancellationToken ct = default)
    {
        var room = await _rooms.GetByIdAsync(roomId, ct);
        if (room is null || room.Status != RoomStatus.Active) return null;

        room.Status = RoomStatus.Ended;
        room.EndedAt = DateTime.UtcNow;
        await _rooms.ReplaceAsync(roomId, room, ct);
        var duration = room.StartedAt.HasValue ? room.EndedAt.Value - room.StartedAt.Value : TimeSpan.Zero;
        _metrics.RoomEnded(duration);
        return room;
    }

    public async Task<bool> CanMemberJoinAsync(string roomId, string memberId, CancellationToken ct = default)
    {
        var room = await _rooms.GetByIdAsync(roomId, ct);
        if (room is null || room.Status != RoomStatus.Active) return false;

        var member = await _members.GetByIdAsync(memberId, ct);
        if (member is null || member.Status != MemberStatus.Active) return false;

        // Member must belong to the same chapter as the room
        return member.ChapterId == room.ChapterId;
    }
}
