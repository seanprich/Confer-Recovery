using System.Security.Claims;

namespace ConferRecovery.Server.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? MemberId(this ClaimsPrincipal user)
        => user.FindFirst("sub")?.Value;

    public static string? ChapterId(this ClaimsPrincipal user)
        => user.FindFirst("chapter")?.Value;

    public static bool IsOrgAdmin(this ClaimsPrincipal user)
        => user.IsInRole("OrgAdmin");

    public static bool IsChapterAdmin(this ClaimsPrincipal user)
        => user.IsInRole("ChapterAdmin");
}
