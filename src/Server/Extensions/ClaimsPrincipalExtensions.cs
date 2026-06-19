using System.Security.Claims;

namespace ConferRecovery.Server.Extensions;

internal static class ClaimsPrincipalExtensions
{
    internal static string? MemberId(this ClaimsPrincipal user)
        => user.FindFirst("sub")?.Value;

    internal static string? ChapterId(this ClaimsPrincipal user)
        => user.FindFirst("chapter")?.Value;

    internal static bool IsOrgAdmin(this ClaimsPrincipal user)
        => user.IsInRole("OrgAdmin");

    internal static bool IsChapterAdmin(this ClaimsPrincipal user)
        => user.IsInRole("ChapterAdmin");
}
