using EphemeralMongo;
using MongoDB.Driver;

namespace ConferRecovery.Tests.Fixtures;

/// <summary>
/// Spins up a real MongoDB process per test class. Each fixture instance gets its
/// own database name to keep tests fully isolated even if xUnit runs classes in parallel.
/// </summary>
public sealed class MongoFixture : IAsyncLifetime
{
    private IMongoRunner? _runner;
    public IMongoDatabase Database { get; private set; } = default!;

    public Task InitializeAsync()
    {
        _runner = MongoRunner.Run(new MongoRunnerOptions
        {
            AdditionalArguments = "--quiet",
        });
        var client = new MongoClient(_runner.ConnectionString);
        Database = client.GetDatabase("confer_test_" + Guid.NewGuid().ToString("N")[..8]);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        // EphemeralMongo6 uses MongoDB.Driver 2.x internally for graceful shutdown,
        // which conflicts with the 3.x reference in this project. Swallow the load error;
        // the mongod process is killed by the OS when the test host exits.
        try { _runner?.Dispose(); }
        catch (FileNotFoundException) { }
        return Task.CompletedTask;
    }
}
