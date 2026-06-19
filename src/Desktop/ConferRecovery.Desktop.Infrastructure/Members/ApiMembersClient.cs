using ConferRecovery.Desktop.Application.Members;
using ConferRecovery.Desktop.Contracts.Members;
using ConferRecovery.Desktop.Infrastructure.Generated;
using ContractCreateMemberRequest = ConferRecovery.Desktop.Contracts.Members.CreateMemberRequest;
using ContractUpdateMemberRoleRequest = ConferRecovery.Desktop.Contracts.Members.UpdateMemberRoleRequest;
using ContractUpdateMemberStatusRequest = ConferRecovery.Desktop.Contracts.Members.UpdateMemberStatusRequest;
using GeneratedCreateMemberRequest = ConferRecovery.Desktop.Infrastructure.Generated.CreateMemberRequest;
using GeneratedUpdateMemberRoleRequest = ConferRecovery.Desktop.Infrastructure.Generated.UpdateMemberRoleRequest;
using GeneratedUpdateMemberStatusRequest = ConferRecovery.Desktop.Infrastructure.Generated.UpdateMemberStatusRequest;

namespace ConferRecovery.Desktop.Infrastructure.Members;

public sealed class ApiMembersClient : IMembersApiClient
{
    private readonly IMembersClient _client;

    public ApiMembersClient(IMembersClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<MemberSummary>> GetByChapterAsync(string chapterId, CancellationToken ct)
    {
        var result = await _client.GetMembersByChapterAsync(chapterId, ct);
        return result.Select(Map).ToList();
    }

    public async Task<MemberSummary?> GetByIdAsync(string id, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetMemberByIdAsync(id, ct);
            return Map(response);
        }
        catch (ConferApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<MemberSummary> CreateAsync(ContractCreateMemberRequest request, CancellationToken ct)
    {
        var response = await _client.CreateMemberAsync(new GeneratedCreateMemberRequest
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            Password = request.Password,
            ChapterId = request.ChapterId,
            Role = request.Role
        }, ct);

        return Map(response);
    }

    public async Task<bool> UpdateStatusAsync(string id, ContractUpdateMemberStatusRequest request, CancellationToken ct)
    {
        try
        {
            await _client.UpdateMemberStatusAsync(id, new GeneratedUpdateMemberStatusRequest
            {
                Status = request.Status
            }, ct);

            return true;
        }
        catch (ConferApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }
    }

    public async Task<bool> UpdateRoleAsync(string id, ContractUpdateMemberRoleRequest request, CancellationToken ct)
    {
        try
        {
            await _client.UpdateMemberRoleAsync(id, new GeneratedUpdateMemberRoleRequest
            {
                Role = request.Role
            }, ct);

            return true;
        }
        catch (ConferApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }
    }

    private static MemberSummary Map(MemberResponse source) => new(
        source.Id,
        source.DisplayName,
        source.Email,
        source.ChapterId,
        source.Role,
        source.Status,
        source.CreatedAt.UtcDateTime,
        source.LastLoginAt?.UtcDateTime,
        source.ConsentAcknowledged);
}