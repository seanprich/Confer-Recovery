using MongoDB.Driver;
using SPQC.Confer.SelfHosted.Server.Models;

namespace SPQC.Confer.SelfHosted.Server.Repositories;

public sealed class MemberRepository : MongoRepository<Member>, IMemberRepository
{
    public MemberRepository(IMongoDatabase database) : base(database, "members")
    {
        var emailIndex = new CreateIndexModel<Member>(
            Builders<Member>.IndexKeys.Ascending(m => m.Email),
            new CreateIndexOptions { Unique = true });
        var chapterIndex = new CreateIndexModel<Member>(
            Builders<Member>.IndexKeys.Ascending(m => m.ChapterId));
        Collection.Indexes.CreateMany([emailIndex, chapterIndex]);
    }

    public async Task<Member?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await FindOneAsync(m => m.Email == email.ToLowerInvariant(), ct);

    public async Task<IReadOnlyList<Member>> GetByChapterAsync(string chapterId, CancellationToken ct = default)
        => await FindAsync(m => m.ChapterId == chapterId, ct);
}
