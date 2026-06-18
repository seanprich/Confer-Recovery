using SPQC.Confer.SelfHosted.Server.Models;

namespace SPQC.Confer.SelfHosted.Server.Repositories;

public interface IMemberRepository : IRepository<Member>
{
    Task<Member?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<Member>> GetByChapterAsync(string chapterId, CancellationToken ct = default);
}
