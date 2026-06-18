using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPQC.Confer.SelfHosted.Server.DTOs.Chapters;
using SPQC.Confer.SelfHosted.Server.Models;
using SPQC.Confer.SelfHosted.Server.Services;

namespace SPQC.Confer.SelfHosted.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "OrgAdmin")]
public sealed class ChaptersController : ControllerBase
{
    private readonly IChapterService _chapters;

    public ChaptersController(IChapterService chapters) => _chapters = chapters;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ChapterResponse>>> GetActive(CancellationToken ct)
    {
        var chapters = await _chapters.GetAllActiveAsync(ct);
        return Ok(chapters.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChapterResponse>> GetById(string id, CancellationToken ct)
    {
        var chapter = await _chapters.GetByIdAsync(id, ct);
        return chapter is null ? NotFound() : Ok(ToResponse(chapter));
    }

    [HttpPost]
    public async Task<ActionResult<ChapterResponse>> Create(
        [FromBody] CreateChapterRequest request, CancellationToken ct)
    {
        var chapter = await _chapters.CreateAsync(
            request.Name, request.SfuUrl, request.LiveKitApiKey, request.LiveKitApiSecret, ct);
        return CreatedAtAction(nameof(GetById), new { id = chapter.Id }, ToResponse(chapter));
    }

    [HttpPut("{id}/sfu")]
    public async Task<IActionResult> UpdateSfu(
        string id, [FromBody] CreateChapterRequest request, CancellationToken ct)
    {
        var updated = await _chapters.UpdateSfuAsync(
            id, request.SfuUrl, request.LiveKitApiKey, request.LiveKitApiSecret, ct);
        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> SetStatus(
        string id, [FromBody] string status, CancellationToken ct)
    {
        if (!Enum.TryParse<ChapterStatus>(status, ignoreCase: true, out var chapterStatus))
            return BadRequest(new { error = $"Unknown status: {status}" });

        return await _chapters.SetStatusAsync(id, chapterStatus, ct) ? NoContent() : NotFound();
    }

    private static ChapterResponse ToResponse(Chapter c) => new(
        c.Id, c.Name, c.SfuUrl, c.LiveKitApiKey,
        c.Status.ToString(), c.CreatedAt);
}
