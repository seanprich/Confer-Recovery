using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Repositories;

public interface IRoomRepository : IRepository<Room>
{
    Task<IReadOnlyList<Room>> GetByChapterAsync(string chapterId, CancellationToken ct = default);
    Task<Room?> GetActiveRoomForChapterAsync(string chapterId, CancellationToken ct = default);
    Task<Room?> GetByLiveKitRoomNameAsync(string liveKitRoomName, CancellationToken ct = default);
}
