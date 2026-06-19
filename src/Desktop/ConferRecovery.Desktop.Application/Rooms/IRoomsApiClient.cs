using ConferRecovery.Desktop.Contracts.Rooms;

namespace ConferRecovery.Desktop.Application.Rooms;

public interface IRoomsApiClient
{
    Task<IReadOnlyList<RoomSummary>> GetByChapterAsync(string chapterId, CancellationToken ct);
    Task<RoomSummary?> GetByIdAsync(string id, CancellationToken ct);
    Task<RoomSummary> CreateAsync(CreateRoomRequest request, CancellationToken ct);
    Task<RoomSummary> StartAsync(string id, CancellationToken ct);
    Task<RoomSummary> EndAsync(string id, CancellationToken ct);
    Task<JoinRoomResponse> JoinAsync(string id, CancellationToken ct);
}