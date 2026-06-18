using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPQC.Confer.SelfHosted.Server.DTOs.Members;
using SPQC.Confer.SelfHosted.Server.Models;
using SPQC.Confer.SelfHosted.Server.Services;

namespace SPQC.Confer.SelfHosted.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MembersController : ControllerBase
{
    private readonly IMemberService _members;

    public MembersController(IMemberService members) => _members = members;

    [HttpGet]
    [Authorize(Roles = "ChapterAdmin,OrgAdmin")]
    public async Task<ActionResult<IReadOnlyList<MemberResponse>>> GetByChapter(
        [FromQuery] string chapterId, CancellationToken ct)
    {
        var members = await _members.GetByChapterAsync(chapterId, ct);
        return Ok(members.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberResponse>> GetById(string id, CancellationToken ct)
    {
        var member = await _members.GetByIdAsync(id, ct);
        if (member is null) return NotFound();

        // Members can only see their own record unless they are admins
        var callerId = User.FindFirst("sub")?.Value;
        var callerRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var isAdmin = callerRole is "ChapterAdmin" or "OrgAdmin";
        if (!isAdmin && callerId != id) return Forbid();

        return Ok(ToResponse(member));
    }

    [HttpPost]
    [Authorize(Roles = "ChapterAdmin,OrgAdmin")]
    public async Task<ActionResult<MemberResponse>> Create(
        [FromBody] CreateMemberRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<MemberRole>(request.Role, ignoreCase: true, out var role))
            return BadRequest(new { error = $"Unknown role: {request.Role}" });

        try
        {
            var member = await _members.CreateAsync(
                request.DisplayName, request.Email, request.Password,
                request.ChapterId, role, ct);
            return CreatedAtAction(nameof(GetById), new { id = member.Id }, ToResponse(member));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "ChapterAdmin,OrgAdmin")]
    public async Task<IActionResult> UpdateStatus(
        string id, [FromBody] UpdateMemberStatusRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<MemberStatus>(request.Status, ignoreCase: true, out var status))
            return BadRequest(new { error = $"Unknown status: {request.Status}" });

        return await _members.UpdateStatusAsync(id, status, ct) ? NoContent() : NotFound();
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "OrgAdmin")]
    public async Task<IActionResult> UpdateRole(
        string id, [FromBody] UpdateMemberRoleRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<MemberRole>(request.Role, ignoreCase: true, out var role))
            return BadRequest(new { error = $"Unknown role: {request.Role}" });

        return await _members.UpdateRoleAsync(id, role, ct) ? NoContent() : NotFound();
    }

    private static MemberResponse ToResponse(Member m) => new(
        m.Id, m.DisplayName, m.Email, m.ChapterId,
        m.Role.ToString(), m.Status.ToString(),
        m.CreatedAt, m.LastLoginAt,
        m.ConsentAcknowledgedAt.HasValue);
}
