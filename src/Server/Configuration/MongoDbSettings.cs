namespace ConferRecovery.Server.Configuration;

public sealed class MongoDbSettings
{
    public string ConnectionString { get; init; } = "mongodb://localhost:27017";
    public string DatabaseName { get; init; } = "confer";
    public string? Username { get; init; }
    public string? Password { get; init; }

    public bool HasCredentials =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}
