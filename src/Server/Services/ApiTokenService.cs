using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ConferRecovery.Server.Configuration;
using ConferRecovery.Server.Models;

namespace ConferRecovery.Server.Services;

public sealed class ApiTokenService : IApiTokenService
{
    private readonly JwtSettings _settings;

    public ApiTokenService(JwtSettings settings) => _settings = settings;

    public ApiToken Issue(Member member)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, member.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim("name", member.DisplayName),
            new Claim("chapter", member.ChapterId),
            new Claim(ClaimTypes.Role, member.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new ApiToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
