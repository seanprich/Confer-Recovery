using ConferRecovery.Desktop.Contracts.Members;

namespace ConferRecovery.Desktop.Application.Members;

public interface IMembersApiClient
{
    Task<IReadOnlyList<MemberSummary>> GetByChapterAsync(string chapterId, CancellationToken ct);
    Task<MemberSummary?> GetByIdAsync(string id, CancellationToken ct);
    Task<MemberSummary> CreateAsync(CreateMemberRequest request, CancellationToken ct);
    Task<bool> UpdateStatusAsync(string id, UpdateMemberStatusRequest request, CancellationToken ct);
    Task<bool> UpdateRoleAsync(string id, UpdateMemberRoleRequest request, CancellationToken ct);
}