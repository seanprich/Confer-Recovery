using ConferRecovery.Desktop.Infrastructure.Generated;
using ConferRecovery.Desktop.Infrastructure.Members;
using NSubstitute;

namespace ConferRecovery.Tests.Desktop.Infrastructure;

public sealed class ApiMembersClientTests
{
    [Fact]
    public async Task GetByChapterAsync_MapsGeneratedCollection()
    {
        var generated = Substitute.For<IMembersClient>();
        var createdAt = new DateTimeOffset(DateTime.UtcNow.AddDays(-1));

        generated.MembersAllAsync("chapter-1", Arg.Any<CancellationToken>())
            .Returns(new List<MemberResponse>
            {
                new()
                {
                    Id = "member-1",
                    DisplayName = "Jane",
                    Email = "jane@example.org",
                    ChapterId = "chapter-1",
                    Role = "Host",
                    Status = "Active",
                    CreatedAt = createdAt,
                    LastLoginAt = null,
                    ConsentAcknowledged = true
                }
            });

        var sut = new ApiMembersClient(generated);

        var result = await sut.GetByChapterAsync("chapter-1", CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("member-1", result[0].Id);
        Assert.Equal(createdAt.UtcDateTime, result[0].CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var generated = Substitute.For<IMembersClient>();
        generated.MembersGETAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<MemberResponse>>(_ => throw new ConferApiException(
                "Not Found",
                404,
                string.Empty,
                new Dictionary<string, IEnumerable<string>>(),
                null!));

        var sut = new ApiMembersClient(generated);

        var result = await sut.GetByIdAsync("missing", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_MapsResponse()
    {
        var generated = Substitute.For<IMembersClient>();
        var createdAt = new DateTimeOffset(DateTime.UtcNow.AddDays(-4));
        var lastLoginAt = new DateTimeOffset(DateTime.UtcNow.AddHours(-2));

        generated.MembersGETAsync("member-1", Arg.Any<CancellationToken>())
            .Returns(new MemberResponse
            {
                Id = "member-1",
                DisplayName = "Jane",
                Email = "jane@example.org",
                ChapterId = "chapter-1",
                Role = "Host",
                Status = "Active",
                CreatedAt = createdAt,
                LastLoginAt = lastLoginAt,
                ConsentAcknowledged = true
            });

        var sut = new ApiMembersClient(generated);

        var result = await sut.GetByIdAsync("member-1", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("member-1", result!.Id);
        Assert.Equal(lastLoginAt.UtcDateTime, result.LastLoginAt);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenSuccessful_ReturnsTrueAndCallsGeneratedClient()
    {
        var generated = Substitute.For<IMembersClient>();
        var sut = new ApiMembersClient(generated);

        var success = await sut.UpdateStatusAsync(
            "member-1",
            new ConferRecovery.Desktop.Contracts.Members.UpdateMemberStatusRequest("Inactive"),
            CancellationToken.None);

        Assert.True(success);
        await generated.Received(1).Status2Async(
            "member-1",
            Arg.Is<UpdateMemberStatusRequest>(x => x.Status == "Inactive"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenNotFound_ReturnsFalse()
    {
        var generated = Substitute.For<IMembersClient>();
        generated.Status2Async(Arg.Any<string>(), Arg.Any<UpdateMemberStatusRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConferApiException(
                "Not Found",
                404,
                string.Empty,
                new Dictionary<string, IEnumerable<string>>(),
                null!));

        var sut = new ApiMembersClient(generated);

        var success = await sut.UpdateStatusAsync(
            "member-1",
            new ConferRecovery.Desktop.Contracts.Members.UpdateMemberStatusRequest("Inactive"),
            CancellationToken.None);

        Assert.False(success);
    }

    [Fact]
    public async Task UpdateRoleAsync_WhenSuccessful_ReturnsTrue()
    {
        var generated = Substitute.For<IMembersClient>();
        var sut = new ApiMembersClient(generated);

        var success = await sut.UpdateRoleAsync(
            "member-1",
            new ConferRecovery.Desktop.Contracts.Members.UpdateMemberRoleRequest("Presenter"),
            CancellationToken.None);

        Assert.True(success);
        await generated.Received(1).RoleAsync(
            "member-1",
            Arg.Is<UpdateMemberRoleRequest>(x => x.Role == "Presenter"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateRoleAsync_WhenNotFound_ReturnsFalse()
    {
        var generated = Substitute.For<IMembersClient>();
        generated.RoleAsync(Arg.Any<string>(), Arg.Any<UpdateMemberRoleRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConferApiException(
                "Not Found",
                404,
                string.Empty,
                new Dictionary<string, IEnumerable<string>>(),
                null!));

        var sut = new ApiMembersClient(generated);

        var success = await sut.UpdateRoleAsync(
            "member-1",
            new ConferRecovery.Desktop.Contracts.Members.UpdateMemberRoleRequest("Presenter"),
            CancellationToken.None);

        Assert.False(success);
    }

    [Fact]
    public async Task CreateAsync_MapsResponseToContract()
    {
        var generated = Substitute.For<IMembersClient>();
        var createdAt = new DateTimeOffset(DateTime.UtcNow.AddDays(-1));

        generated.MembersPOSTAsync(Arg.Any<CreateMemberRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MemberResponse
            {
                Id = "member-1",
                DisplayName = "Jane",
                Email = "jane@example.org",
                ChapterId = "chapter-1",
                Role = "Host",
                Status = "Active",
                CreatedAt = createdAt,
                LastLoginAt = null,
                ConsentAcknowledged = true
            });

        var sut = new ApiMembersClient(generated);

        var result = await sut.CreateAsync(
            new ConferRecovery.Desktop.Contracts.Members.CreateMemberRequest(
                "Jane",
                "jane@example.org",
                "long-password",
                "chapter-1",
                "Host"),
            CancellationToken.None);

        Assert.Equal("member-1", result.Id);
        Assert.Equal("Host", result.Role);
        Assert.Equal(createdAt.UtcDateTime, result.CreatedAt);
    }
}