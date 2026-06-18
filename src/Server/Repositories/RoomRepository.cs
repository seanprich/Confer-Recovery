using MongoDB.Driver;
using SPQC.Confer.SelfHosted.Server.Models;

namespace SPQC.Confer.SelfHosted.Server.Repositories;

public sealed class RoomRepository : MongoRepository<Room>, IRoomRepository
{
    public RoomRepository(IMongoDatabase database) : base(database, "rooms")
    {
        var chapterStatusIndex = new CreateIndexModel<Room>(
            Builders<Room>.IndexKeys.Ascending(r => r.ChapterId).Ascending(r => r.Status));
        var lkRoomIndex = new CreateIndexModel<Room>(
            Builders<Room>.IndexKeys.Ascending(r => r.LiveKitRoomName),
            new CreateIndexOptions { Unique = true });
        Collection.Indexes.CreateMany([chapterStatusIndex, lkRoomIndex]);
    }

    public async Task<IReadOnlyList<Room>> GetByChapterAsync(string chapterId, CancellationToken ct = default)
        => await FindAsync(r => r.ChapterId == chapterId, ct);

    public async Task<Room?> GetActiveRoomForChapterAsync(string chapterId, CancellationToken ct = default)
        => await FindOneAsync(r => r.ChapterId == chapterId && r.Status == RoomStatus.Active, ct);

    public async Task<Room?> GetByLiveKitRoomNameAsync(string liveKitRoomName, CancellationToken ct = default)
        => await FindOneAsync(r => r.LiveKitRoomName == liveKitRoomName, ct);
}
