namespace ConferRecovery.Server.Telemetry;

public interface IConferMetrics
{
    void RoomCreated();
    void RoomStarted();
    void RoomEnded(TimeSpan duration);
    void TokenIssued(string role);
    void AuthAttempt(bool success);
    void AuditEventRecorded(string eventType);
}
