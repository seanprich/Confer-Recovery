using SPQC.Confer.SelfHosted.Server.Models;

namespace SPQC.Confer.SelfHosted.Server.Services;

public interface IMemberService
{
    Task<Member?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Member>> GetByChapterAsync(string chapterId, CancellationToken ct = default);
    Task<Member> CreateAsync(string displayName, string email, string password, string chapterId,
        MemberRole role = MemberRole.Listener, CancellationToken ct = default);
    Task<Member?> AuthenticateAsync(string email, string password, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(string id, MemberStatus status, CancellationToken ct = default);
    Task<bool> UpdateRoleAsync(string id, MemberRole role, CancellationToken ct = default);
    Task RecordConsentAsync(string id, string consentVersion, CancellationToken ct = default);
    Task RecordLoginAsync(string id, CancellationToken ct = default);
}
