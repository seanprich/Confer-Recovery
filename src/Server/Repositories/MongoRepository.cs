using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ConferRecovery.Server.Repositories;

public class MongoRepository<T> : IRepository<T>
{
    protected readonly IMongoCollection<T> Collection;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        Collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        return await Collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await Collection.Find(Builders<T>.Filter.Empty).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await Collection.Find(predicate).ToListAsync(ct);
    }

    public async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await Collection.Find(predicate).FirstOrDefaultAsync(ct);
    }

    public async Task InsertAsync(T entity, CancellationToken ct = default)
    {
        await Collection.InsertOneAsync(entity, cancellationToken: ct);
    }

    public async Task<bool> ReplaceAsync(string id, T entity, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        var result = await Collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        var result = await Collection.DeleteOneAsync(filter, ct);
        return result.DeletedCount > 0;
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await Collection.CountDocumentsAsync(predicate, cancellationToken: ct);
    }
}
