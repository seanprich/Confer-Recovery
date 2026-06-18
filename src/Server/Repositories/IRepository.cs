using System.Linq.Expressions;

namespace SPQC.Confer.SelfHosted.Server.Repositories;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task InsertAsync(T entity, CancellationToken ct = default);
    Task<bool> ReplaceAsync(string id, T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task<long> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
}
