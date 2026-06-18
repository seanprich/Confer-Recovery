using SPQC.Confer.SelfHosted.Server.Models;

namespace SPQC.Confer.SelfHosted.Server.Repositories;

public interface IChapterRepository : IRepository<Chapter>
{
    Task<IReadOnlyList<Chapter>> GetActiveAsync(CancellationToken ct = default);
}
