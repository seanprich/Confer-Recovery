using ConferRecovery.Desktop.Application.Chapters;
using ConferRecovery.Desktop.Contracts.Chapters;
using ConferRecovery.Desktop.Infrastructure.Generated;
using ContractCreateChapterRequest = ConferRecovery.Desktop.Contracts.Chapters.CreateChapterRequest;
using GeneratedCreateChapterRequest = ConferRecovery.Desktop.Infrastructure.Generated.CreateChapterRequest;

namespace ConferRecovery.Desktop.Infrastructure.Chapters;

public sealed class ApiChaptersClient : IChaptersApiClient
{
    private readonly IChaptersClient _client;

    public ApiChaptersClient(IChaptersClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<ChapterSummary>> GetActiveAsync(CancellationToken ct)
    {
        var result = await _client.GetActiveChaptersAsync(ct);
        return result.Select(Map).ToList();
    }

    public async Task<ChapterSummary?> GetByIdAsync(string id, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetChapterByIdAsync(id, ct);
            return Map(response);
        }
        catch (ConferApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<ChapterSummary> CreateAsync(ContractCreateChapterRequest request, CancellationToken ct)
    {
        var response = await _client.CreateChapterAsync(new GeneratedCreateChapterRequest
        {
            Name = request.Name,
            SfuUrl = request.SfuUrl,
            LiveKitApiKey = request.LiveKitApiKey,
            LiveKitApiSecret = request.LiveKitApiSecret
        }, ct);

        return Map(response);
    }

    public async Task<bool> UpdateSfuAsync(string id, ContractCreateChapterRequest request, CancellationToken ct)
    {
        try
        {
            await _client.UpdateChapterSfuAsync(id, new GeneratedCreateChapterRequest
            {
                Name = request.Name,
                SfuUrl = request.SfuUrl,
                LiveKitApiKey = request.LiveKitApiKey,
                LiveKitApiSecret = request.LiveKitApiSecret
            }, ct);

            return true;
        }
        catch (ConferApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }
    }

    public async Task<bool> SetStatusAsync(string id, string status, CancellationToken ct)
    {
        try
        {
            await _client.SetChapterStatusAsync(id, status, ct);
            return true;
        }
        catch (ConferApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }
    }

    private static ChapterSummary Map(ChapterResponse source) => new(
        source.Id,
        source.Name,
        source.SfuUrl,
        source.LiveKitApiKey,
        source.Status,
        source.CreatedAt.UtcDateTime);
}