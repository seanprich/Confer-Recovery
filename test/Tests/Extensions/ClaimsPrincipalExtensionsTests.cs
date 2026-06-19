using System.Security.Claims;
using ConferRecovery.Server.Extensions;

namespace ConferRecovery.Tests.Extensions;

public sealed class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal Make(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));

    [Fact]
    public void MemberId_ReturnsSub()
    {
        var user = Make(new Claim("sub", "member-42"));
        Assert.Equal("member-42", user.MemberId());
    }

    [Fact]
    public void MemberId_WhenMissing_ReturnsNull()
    {
        var user = Make();
        Assert.Null(user.MemberId());
    }

    [Fact]
    public void ChapterId_ReturnsChapterClaim()
    {
        var user = Make(new Claim("chapter", "chap-99"));
        Assert.Equal("chap-99", user.ChapterId());
    }

    [Fact]
    public void ChapterId_WhenMissing_ReturnsNull()
    {
        var user = Make();
        Assert.Null(user.ChapterId());
    }

    [Theory]
    [InlineData("OrgAdmin", true)]
    [InlineData("ChapterAdmin", false)]
    [InlineData("Host", false)]
    public void IsOrgAdmin_MatchesRole(string role, bool expected)
    {
        var user = Make(new Claim(ClaimTypes.Role, role));
        Assert.Equal(expected, user.IsOrgAdmin());
    }

    [Theory]
    [InlineData("ChapterAdmin", true)]
    [InlineData("OrgAdmin", false)]
    [InlineData("Host", false)]
    public void IsChapterAdmin_MatchesRole(string role, bool expected)
    {
        var user = Make(new Claim(ClaimTypes.Role, role));
        Assert.Equal(expected, user.IsChapterAdmin());
    }
}
