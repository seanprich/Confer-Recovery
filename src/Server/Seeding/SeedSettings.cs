namespace ConferRecovery.Server.Seeding;

public sealed class SeedSettings
{
    public string? AdminEmail { get; init; }
    public string? AdminPassword { get; init; }
    public string AdminDisplayName { get; init; } = "Admin";
    public string ChapterName { get; init; } = "Default Chapter";
    public string SfuUrl { get; init; } = string.Empty;
    public string LiveKitApiKey { get; init; } = string.Empty;
    public string LiveKitApiSecret { get; init; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(AdminEmail) &&
        !string.IsNullOrWhiteSpace(AdminPassword);
}
