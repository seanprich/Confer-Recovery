using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConferRecovery.Desktop.Infrastructure.Generated;

// Volunteer-friendly aliases over generated NSwag method names.
public static class ConferApiClientFriendlyExtensions
{
    public static Task<LoginResponse> LoginWithCredentialsAsync(
        this IAuthClient client,
        LoginRequest body,
        CancellationToken cancellationToken = default)
        => client.LoginAsync(body, cancellationToken);

    public static Task AcknowledgeConsentAsync(
        this IAuthClient client,
        string consentVersion,
        CancellationToken cancellationToken = default)
        => client.ConsentAsync(consentVersion, cancellationToken);

    public static Task<ICollection<ChapterResponse>> GetActiveChaptersAsync(
        this IChaptersClient client,
        CancellationToken cancellationToken = default)
        => client.ChaptersAllAsync(cancellationToken);

    public static Task<ChapterResponse> GetChapterByIdAsync(
        this IChaptersClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.ChaptersGETAsync(id, cancellationToken);

    public static Task<ChapterResponse> CreateChapterAsync(
        this IChaptersClient client,
        CreateChapterRequest body,
        CancellationToken cancellationToken = default)
        => client.ChaptersPOSTAsync(body, cancellationToken);

    public static Task UpdateChapterSfuAsync(
        this IChaptersClient client,
        string id,
        CreateChapterRequest body,
        CancellationToken cancellationToken = default)
        => client.SfuAsync(id, body, cancellationToken);

    public static Task SetChapterStatusAsync(
        this IChaptersClient client,
        string id,
        string status,
        CancellationToken cancellationToken = default)
        => client.StatusAsync(id, status, cancellationToken);

    public static Task<ICollection<MemberResponse>> GetMembersByChapterAsync(
        this IMembersClient client,
        string chapterId,
        CancellationToken cancellationToken = default)
        => client.MembersAllAsync(chapterId, cancellationToken);

    public static Task<MemberResponse> GetMemberByIdAsync(
        this IMembersClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.MembersGETAsync(id, cancellationToken);

    public static Task<MemberResponse> CreateMemberAsync(
        this IMembersClient client,
        CreateMemberRequest body,
        CancellationToken cancellationToken = default)
        => client.MembersPOSTAsync(body, cancellationToken);

    public static Task UpdateMemberStatusAsync(
        this IMembersClient client,
        string id,
        UpdateMemberStatusRequest body,
        CancellationToken cancellationToken = default)
        => client.Status2Async(id, body, cancellationToken);

    public static Task UpdateMemberRoleAsync(
        this IMembersClient client,
        string id,
        UpdateMemberRoleRequest body,
        CancellationToken cancellationToken = default)
        => client.RoleAsync(id, body, cancellationToken);

    public static Task<ICollection<RoomResponse>> GetRoomsByChapterAsync(
        this IRoomsClient client,
        string chapterId,
        CancellationToken cancellationToken = default)
        => client.RoomsAllAsync(chapterId, cancellationToken);

    public static Task<RoomResponse> GetRoomByIdAsync(
        this IRoomsClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.RoomsGETAsync(id, cancellationToken);

    public static Task<RoomResponse> CreateRoomAsync(
        this IRoomsClient client,
        CreateRoomRequest body,
        CancellationToken cancellationToken = default)
        => client.RoomsPOSTAsync(body, cancellationToken);

    public static Task<RoomResponse> StartRoomAsync(
        this IRoomsClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.StartAsync(id, cancellationToken);

    public static Task<RoomResponse> EndRoomAsync(
        this IRoomsClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.EndAsync(id, cancellationToken);

    public static Task<JoinRoomResponse> JoinRoomAsync(
        this IRoomsClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.JoinAsync(id, cancellationToken);
}
