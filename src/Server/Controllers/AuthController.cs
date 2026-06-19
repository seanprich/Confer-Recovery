using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConferRecovery.Server.DTOs.Auth;
using ConferRecovery.Server.DTOs.Members;
using ConferRecovery.Server.Services;

namespace ConferRecovery.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IMemberService _members;
    private readonly IApiTokenService _tokens;
    private readonly IAuditService _audit;

    public AuthController(IMemberService members, IApiTokenService tokens, IAuditService audit)
    {
        _members = members;
        _tokens = tokens;
        _audit = audit;
    }

    [HttpPost("login", Name = "AuthLogin")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var member = await _members.AuthenticateAsync(request.Email, request.Password, ct);
        if (member is null)
            return Unauthorized(new { error = "Invalid credentials or account not active." });

        await _members.RecordLoginAsync(member.Id, ct);

        var token = _tokens.Issue(member);
        return Ok(new LoginResponse(
            AccessToken: token.Value,
            ExpiresAt: token.ExpiresAt,
            MemberId: member.Id,
            DisplayName: member.DisplayName,
            ChapterId: member.ChapterId,
            Role: member.Role.ToString()));
    }

    [HttpPost("consent", Name = "AuthAcknowledgeConsent")]
    [Authorize]
    public async Task<IActionResult> AcknowledgeConsent(
        [FromBody] AcknowledgeConsentRequest request, CancellationToken ct)
    {
        var memberId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        if (memberId is null) return Unauthorized();

        var member = await _members.GetByIdAsync(memberId, ct);
        if (member is null) return NotFound();

        await _members.RecordConsentAsync(memberId, request.ConsentVersion, ct);

        // No room context at consent time — use a sentinel room ID for the audit entry
        await _audit.RecordAsync("none", memberId, member.DisplayName,
            Models.AuditEventType.ConsentAcknowledged,
            new Dictionary<string, string> { ["version"] = request.ConsentVersion }, ct);

        return NoContent();
    }
}
