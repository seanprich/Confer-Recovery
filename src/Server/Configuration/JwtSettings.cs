namespace ConferRecovery.Server.Configuration;

public sealed class JwtSettings
{
    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = "confer";
    public string Audience { get; init; } = "confer-clients";
    public int ExpiryMinutes { get; init; } = 60;
}
