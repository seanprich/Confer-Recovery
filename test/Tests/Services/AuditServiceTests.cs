using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ConferRecovery.Server.Models;
using ConferRecovery.Server.Repositories;
using ConferRecovery.Server.Services;
using ConferRecovery.Server.Telemetry;

namespace ConferRecovery.Tests.Services;

public sealed class AuditServiceTests
{
    private readonly IAuditRepository _repo = Substitute.For<IAuditRepository>();
    private readonly IConferMetrics _metrics = Substitute.For<IConferMetrics>();
    private AuditService Sut() => new(_repo, NullLogger<AuditService>.Instance, _metrics);

    [Fact]
    public async Task RecordAsync_InsertsEventWithCorrectFields()
    {
        AuditEvent? captured = null;
        await _repo.InsertAsync(Arg.Do<AuditEvent>(e => captured = e), default);

        await Sut().RecordAsync("roomId", "memberId", "Jane Doe", AuditEventType.Joined);

        Assert.NotNull(captured);
        Assert.Equal("roomId", captured!.RoomId);
        Assert.Equal("memberId", captured.MemberId);
        Assert.Equal("Jane Doe", captured.MemberDisplayName);
        Assert.Equal(AuditEventType.Joined, captured.EventType);
    }

    [Fact]
    public async Task RecordAsync_TimestampIsRecentUtc()
    {
        AuditEvent? captured = null;
        await _repo.InsertAsync(Arg.Do<AuditEvent>(e => captured = e), default);
        var before = DateTime.UtcNow;

        await Sut().RecordAsync("r", "m", "Name", AuditEventType.Left);

        Assert.True(captured!.Timestamp >= before.AddSeconds(-1));
        Assert.True(captured.Timestamp <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task RecordAsync_WithMetadata_PassesThroughToEvent()
    {
        AuditEvent? captured = null;
        await _repo.InsertAsync(Arg.Do<AuditEvent>(e => captured = e), default);
        var meta = new Dictionary<string, string> { ["version"] = "v2" };

        await Sut().RecordAsync("r", "m", "Name", AuditEventType.ConsentAcknowledged, meta);

        Assert.Equal("v2", captured!.Metadata!["version"]);
    }
}
