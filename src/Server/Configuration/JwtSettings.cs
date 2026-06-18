namespace ConferRecovery.Server.Configuration;

public sealed class JwtSettings
{
    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = "spqc-confer";
    public string Audience { get; init; } = "spqc-confer-clients";
    public int ExpiryMinutes { get; init; } = 60;
}
