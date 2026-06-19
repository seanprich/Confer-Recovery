using ConferRecovery.Server.Models;
using ConferRecovery.Server.Repositories;
using ConferRecovery.Tests.Fixtures;

namespace ConferRecovery.Tests.Repositories;

/// <summary>
/// Verifies that MongoRepository returns null/false for malformed ObjectId strings
/// instead of throwing FormatException (which would surface as HTTP 500).
/// </summary>
public sealed class MongoRepositoryInvalidIdTests : IAsyncLifetime
{
    private readonly MongoFixture _mongo = new();
    private MemberRepository Repo() => new(_mongo.Database);

    public Task InitializeAsync() => _mongo.InitializeAsync();
    public Task DisposeAsync() => _mongo.DisposeAsync();

    [Theory]
    [InlineData("")]
    [InlineData("not-an-objectid")]
    [InlineData("123")]
    [InlineData("gggggggggggggggggggggggg")]
    public async Task GetByIdAsync_WithInvalidObjectId_ReturnsNull(string id)
    {
        var result = await Repo().GetByIdAsync(id);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-objectid")]
    public async Task ReplaceAsync_WithInvalidObjectId_ReturnsFalse(string id)
    {
        var result = await Repo().ReplaceAsync(id, new Member());
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-objectid")]
    public async Task DeleteAsync_WithInvalidObjectId_ReturnsFalse(string id)
    {
        var result = await Repo().DeleteAsync(id);
        Assert.False(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidButAbsentId_ReturnsNull()
    {
        var result = await Repo().GetByIdAsync("507f1f77bcf86cd799439011");
        Assert.Null(result);
    }
}
