using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SPQC.Confer.SelfHosted.Server.Models;
using SPQC.Confer.SelfHosted.Server.Repositories;
using SPQC.Confer.SelfHosted.Server.Services;
using SPQC.Confer.SelfHosted.Server.Telemetry;

namespace SPQC.Confer.SelfHosted.Tests.Services;

public sealed class MemberServiceTests
{
    private readonly IMemberRepository _repo = Substitute.For<IMemberRepository>();
    private readonly IConferMetrics _metrics = Substitute.For<IConferMetrics>();
    private MemberService Sut() => new(_repo, NullLogger<MemberService>.Instance, _metrics);

    [Fact]
    public async Task CreateAsync_WithNewEmail_InsertsMemberWithHashedPassword()
    {
        _repo.GetByEmailAsync("alice@example.com", default).Returns((Member?)null);

        var member = await Sut().CreateAsync("Alice", "alice@example.com", "password123456", "chapId");

        await _repo.Received(1).InsertAsync(Arg.Is<Member>(m =>
            m.DisplayName == "Alice" &&
            m.Email == "alice@example.com" &&
            m.PasswordHash != "password123456" &&
            BCrypt.Net.BCrypt.Verify("password123456", m.PasswordHash)), default);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_Throws()
    {
        var existing = new Member { Email = "alice@example.com" };
        _repo.GetByEmailAsync("alice@example.com", default).Returns(existing);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Sut().CreateAsync("Alice", "alice@example.com", "password123456", "chapId"));
    }

    [Fact]
    public async Task CreateAsync_NormalizesEmailToLowercase()
    {
        _repo.GetByEmailAsync("alice@example.com", default).Returns((Member?)null);

        await Sut().CreateAsync("Alice", "ALICE@Example.COM", "password123456", "chapId");

        await _repo.Received(1).InsertAsync(
            Arg.Is<Member>(m => m.Email == "alice@example.com"), default);
    }

    [Fact]
    public async Task AuthenticateAsync_WithCorrectPassword_ReturnsMember()
    {
        var member = new Member
        {
            Status = MemberStatus.Active,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-pass")
        };
        _repo.GetByEmailAsync("user@example.com", default).Returns(member);

        var result = await Sut().AuthenticateAsync("user@example.com", "correct-pass");

        Assert.NotNull(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithWrongPassword_ReturnsNull()
    {
        var member = new Member
        {
            Status = MemberStatus.Active,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-pass")
        };
        _repo.GetByEmailAsync("user@example.com", default).Returns(member);

        var result = await Sut().AuthenticateAsync("user@example.com", "wrong-pass");

        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithSuspendedMember_ReturnsNull()
    {
        var member = new Member
        {
            Status = MemberStatus.Suspended,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass")
        };
        _repo.GetByEmailAsync("user@example.com", default).Returns(member);

        var result = await Sut().AuthenticateAsync("user@example.com", "pass");

        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUnknownEmail_ReturnsNull()
    {
        _repo.GetByEmailAsync(Arg.Any<string>(), default).Returns((Member?)null);

        var result = await Sut().AuthenticateAsync("nobody@example.com", "pass");

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithExistingMember_CallsReplace()
    {
        var member = new Member { Id = "abc", Status = MemberStatus.Pending };
        _repo.GetByIdAsync("abc", default).Returns(member);
        _repo.ReplaceAsync("abc", Arg.Any<Member>(), default).Returns(true);

        var result = await Sut().UpdateStatusAsync("abc", MemberStatus.Active);

        Assert.True(result);
        await _repo.Received(1).ReplaceAsync("abc",
            Arg.Is<Member>(m => m.Status == MemberStatus.Active), default);
    }

    [Fact]
    public async Task RecordConsentAsync_SetsConsentFieldsAndVersion()
    {
        var member = new Member { Id = "abc" };
        _repo.GetByIdAsync("abc", default).Returns(member);

        await Sut().RecordConsentAsync("abc", "v1.2");

        await _repo.Received(1).ReplaceAsync("abc",
            Arg.Is<Member>(m =>
                m.ConsentAcknowledgedAt.HasValue &&
                m.ConsentVersion == "v1.2"), default);
    }
}
