using ConferRecovery.Desktop.Application.Rooms;
using ConferRecovery.Desktop.Contracts.Rooms;
using ConferRecovery.Desktop.Infrastructure.Generated;
using ContractCreateRoomRequest = ConferRecovery.Desktop.Contracts.Rooms.CreateRoomRequest;
using ContractJoinRoomResponse = ConferRecovery.Desktop.Contracts.Rooms.JoinRoomResponse;
using GeneratedCreateRoomRequest = ConferRecovery.Desktop.Infrastructure.Generated.CreateRoomRequest;

namespace ConferRecovery.Desktop.Infrastructure.Rooms;

public sealed class ApiRoomsClient : IRoomsApiClient
{
    private readonly IRoomsClient _client;

    public ApiRoomsClient(IRoomsClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<RoomSummary>> GetByChapterAsync(string chapterId, CancellationToken ct)
    {
        var result = await _client.GetRoomsByChapterAsync(chapterId, ct);
        return result.Select(Map).ToList();
    }

    public async Task<RoomSummary?> GetByIdAsync(string id, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetRoomByIdAsync(id, ct);
            return Map(response);
        }
        catch (ConferApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<RoomSummary> CreateAsync(ContractCreateRoomRequest request, CancellationToken ct)
    {
        var response = await _client.CreateRoomAsync(new GeneratedCreateRoomRequest
        {
            Name = request.Name,
            ChapterId = request.ChapterId,
            ScheduledAt = request.ScheduledAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(request.ScheduledAt.Value, DateTimeKind.Utc))
                : null
        }, ct);

        return Map(response);
    }

    public async Task<RoomSummary> StartAsync(string id, CancellationToken ct)
    {
        var response = await _client.StartRoomAsync(id, ct);
        return Map(response);
    }

    public async Task<RoomSummary> EndAsync(string id, CancellationToken ct)
    {
        var response = await _client.EndRoomAsync(id, ct);
        return Map(response);
    }

    public async Task<ContractJoinRoomResponse> JoinAsync(string id, CancellationToken ct)
    {
        var response = await _client.JoinRoomAsync(id, ct);
        return new ContractJoinRoomResponse(
            response.LiveKitToken,
            response.SfuUrl,
            response.RoomName,
            response.TokenExpiresAt.UtcDateTime);
    }

    private static RoomSummary Map(RoomResponse source) => new(
        source.Id,
        source.ChapterId,
        source.Name,
        source.HostMemberId,
        source.Status,
        source.ScheduledAt?.UtcDateTime,
        source.StartedAt?.UtcDateTime,
        source.EndedAt?.UtcDateTime,
        source.LobbyEnabled,
        source.MaxVideoPublishers,
        source.MaxParticipants);
}