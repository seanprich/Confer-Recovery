using ConferRecovery.Server.Models;
using ConferRecovery.Server.Repositories;
using ConferRecovery.Server.Telemetry;

namespace ConferRecovery.Server.Services;

public sealed class MemberService : IMemberService
{
    private readonly IMemberRepository _members;
    private readonly ILogger<MemberService> _logger;
    private readonly IConferMetrics _metrics;

    public MemberService(IMemberRepository members, ILogger<MemberService> logger, IConferMetrics metrics)
    {
        _members = members;
        _logger = logger;
        _metrics = metrics;
    }

    public Task<Member?> GetByIdAsync(string id, CancellationToken ct = default)
        => _members.GetByIdAsync(id, ct);

    public Task<IReadOnlyList<Member>> GetByChapterAsync(string chapterId, CancellationToken ct = default)
        => _members.GetByChapterAsync(chapterId, ct);

    public async Task<Member> CreateAsync(string displayName, string email, string password, string chapterId,
        MemberRole role = MemberRole.Listener, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var existing = await _members.GetByEmailAsync(normalizedEmail, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Email {email} is already registered.");

        var member = new Member
        {
            DisplayName = displayName,
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            ChapterId = chapterId,
            Role = role,
            Status = MemberStatus.Pending
        };

        await _members.InsertAsync(member, ct);
        _logger.LogInformation("Member created: {MemberId} ({Email})", member.Id, normalizedEmail);
        return member;
    }

    public async Task<Member?> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        var member = await _members.GetByEmailAsync(email.ToLowerInvariant(), ct);
        if (member is null || member.Status != MemberStatus.Active)
            return null;

        var authenticated = BCrypt.Net.BCrypt.Verify(password, member.PasswordHash);
        _metrics.AuthAttempt(authenticated);
        return authenticated ? member : null;
    }

    public async Task<bool> UpdateStatusAsync(string id, MemberStatus status, CancellationToken ct = default)
    {
        var member = await _members.GetByIdAsync(id, ct);
        if (member is null) return false;
        member.Status = status;
        return await _members.ReplaceAsync(id, member, ct);
    }

    public async Task<bool> UpdateRoleAsync(string id, MemberRole role, CancellationToken ct = default)
    {
        var member = await _members.GetByIdAsync(id, ct);
        if (member is null) return false;
        member.Role = role;
        return await _members.ReplaceAsync(id, member, ct);
    }

    public async Task RecordConsentAsync(string id, string consentVersion, CancellationToken ct = default)
    {
        var member = await _members.GetByIdAsync(id, ct);
        if (member is null) return;
        member.ConsentAcknowledgedAt = DateTime.UtcNow;
        member.ConsentVersion = consentVersion;
        await _members.ReplaceAsync(id, member, ct);
    }

    public async Task RecordLoginAsync(string id, CancellationToken ct = default)
    {
        var member = await _members.GetByIdAsync(id, ct);
        if (member is null) return;
        member.LastLoginAt = DateTime.UtcNow;
        await _members.ReplaceAsync(id, member, ct);
    }
}
