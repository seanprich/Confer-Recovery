using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Services;

public interface IRoomService
{
    Task<Room?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Room>> GetByChapterAsync(string chapterId, CancellationToken ct = default);
    Task<Room> CreateAsync(string chapterId, string hostMemberId, string name,
        DateTime? scheduledAt = null, CancellationToken ct = default);
    Task<Room?> StartAsync(string roomId, CancellationToken ct = default);
    Task<Room?> EndAsync(string roomId, CancellationToken ct = default);
    Task<bool> CanMemberJoinAsync(string roomId, string memberId, CancellationToken ct = default);
}
