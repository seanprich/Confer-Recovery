using MongoDB.Driver;
using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Repositories;

public sealed class ChapterRepository : MongoRepository<Chapter>, IChapterRepository
{
    public ChapterRepository(IMongoDatabase database) : base(database, "chapters") { }

    public async Task<IReadOnlyList<Chapter>> GetActiveAsync(CancellationToken ct = default)
        => await FindAsync(c => c.Status == ChapterStatus.Active, ct);
}
