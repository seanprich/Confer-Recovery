using System.Diagnostics.Metrics;

namespace SPQC.Confer.SelfHosted.Server.Telemetry;

public sealed class ConferMetrics : IConferMetrics
{
    public const string MeterName = "SPQC.Confer";

    private readonly UpDownCounter<int> _activeRooms;
    private readonly Counter<int> _roomsCreated;
    private readonly Histogram<double> _roomDuration;
    private readonly Counter<int> _tokensIssued;
    private readonly Counter<int> _authAttempts;
    private readonly Counter<int> _auditEvents;

    public ConferMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName, "1.0");

        _activeRooms = meter.CreateUpDownCounter<int>(
            "confer.rooms.active", description: "Currently active room sessions");
        _roomsCreated = meter.CreateCounter<int>(
            "confer.rooms.created", description: "Total rooms created");
        _roomDuration = meter.CreateHistogram<double>(
            "confer.room.duration", "s", "Duration of ended room sessions");
        _tokensIssued = meter.CreateCounter<int>(
            "confer.tokens.issued", description: "LiveKit tokens issued");
        _authAttempts = meter.CreateCounter<int>(
            "confer.auth.attempts", description: "Authentication attempts");
        _auditEvents = meter.CreateCounter<int>(
            "confer.audit.events", description: "Audit events recorded");
    }

    public void RoomCreated() =>
        _roomsCreated.Add(1);

    public void RoomStarted() =>
        _activeRooms.Add(1);

    public void RoomEnded(TimeSpan duration)
    {
        _activeRooms.Add(-1);
        _roomDuration.Record(duration.TotalSeconds);
    }

    public void TokenIssued(string role) =>
        _tokensIssued.Add(1, new KeyValuePair<string, object?>("role", role));

    public void AuthAttempt(bool success) =>
        _authAttempts.Add(1, new KeyValuePair<string, object?>("result", success ? "success" : "failure"));

    public void AuditEventRecorded(string eventType) =>
        _auditEvents.Add(1, new KeyValuePair<string, object?>("event_type", eventType));
}
