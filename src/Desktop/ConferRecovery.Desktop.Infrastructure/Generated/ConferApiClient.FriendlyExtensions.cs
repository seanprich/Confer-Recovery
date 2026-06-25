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
        => client.AuthLoginAsync(body, cancellationToken);

    public static Task AcknowledgeConsentAsync(
        this IAuthClient client,
        string consentVersion,
        CancellationToken cancellationToken = default)
        => client.AuthAcknowledgeConsentAsync(new AcknowledgeConsentRequest { ConsentVersion = consentVersion }, cancellationToken);

    public static Task<ICollection<ChapterResponse>> GetActiveChaptersAsync(
        this IChaptersClient client,
        CancellationToken cancellationToken = default)
        => client.ChaptersGetActiveAsync(cancellationToken);

    public static Task<ChapterResponse> GetChapterByIdAsync(
        this IChaptersClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.ChaptersGetByIdAsync(id, cancellationToken);

    public static Task<ChapterResponse> CreateChapterAsync(
        this IChaptersClient client,
        CreateChapterRequest body,
        CancellationToken cancellationToken = default)
        => client.ChaptersCreateAsync(body, cancellationToken);

    public static Task UpdateChapterSfuAsync(
        this IChaptersClient client,
        string id,
        CreateChapterRequest body,
        CancellationToken cancellationToken = default)
        => client.ChaptersUpdateSfuAsync(id, body, cancellationToken);

    public static Task SetChapterStatusAsync(
        this IChaptersClient client,
        string id,
        string status,
        CancellationToken cancellationToken = default)
        => client.ChaptersSetStatusAsync(id, status, cancellationToken);

    public static Task<ICollection<MemberResponse>> GetMembersByChapterAsync(
        this IMembersClient client,
        string chapterId,
        CancellationToken cancellationToken = default)
        => client.MembersGetByChapterAsync(chapterId, cancellationToken);

    public static Task<MemberResponse> GetMemberByIdAsync(
        this IMembersClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.MembersGetByIdAsync(id, cancellationToken);

    public static Task<MemberResponse> CreateMemberAsync(
        this IMembersClient client,
        CreateMemberRequest body,
        CancellationToken cancellationToken = default)
        => client.MembersCreateAsync(body, cancellationToken);

    public static Task UpdateMemberStatusAsync(
        this IMembersClient client,
        string id,
        UpdateMemberStatusRequest body,
        CancellationToken cancellationToken = default)
        => client.MembersUpdateStatusAsync(id, body, cancellationToken);

    public static Task UpdateMemberRoleAsync(
        this IMembersClient client,
        string id,
        UpdateMemberRoleRequest body,
        CancellationToken cancellationToken = default)
        => client.MembersUpdateRoleAsync(id, body, cancellationToken);

    public static Task<ICollection<RoomResponse>> GetRoomsByChapterAsync(
        this IRoomsClient client,
        string chapterId,
        CancellationToken cancellationToken = default)
        => client.RoomsGetByChapterAsync(chapterId, cancellationToken);

    public static Task<RoomResponse> GetRoomByIdAsync(
        this IRoomsClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.RoomsGetByIdAsync(id, cancellationToken);

    public static Task<RoomResponse> CreateRoomAsync(
        this IRoomsClient client,
        CreateRoomRequest body,
        CancellationToken cancellationToken = default)
        => client.RoomsCreateAsync(body, cancellationToken);

    public static Task<RoomResponse> StartRoomAsync(
        this IRoomsClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.RoomsStartAsync(id, cancellationToken);

    public static Task<RoomResponse> EndRoomAsync(
        this IRoomsClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.RoomsEndAsync(id, cancellationToken);

    public static Task<JoinRoomResponse> JoinRoomAsync(
        this IRoomsClient client,
        string id,
        CancellationToken cancellationToken = default)
        => client.RoomsJoinAsync(id, cancellationToken);
}
