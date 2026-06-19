using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Repositories;

public interface IChapterRepository : IRepository<Chapter>
{
    Task<IReadOnlyList<Chapter>> GetActiveAsync(CancellationToken ct = default);
}
