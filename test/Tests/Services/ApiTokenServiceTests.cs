using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ConferRecovery.Server.Configuration;
using ConferRecovery.Server.Models;
using ConferRecovery.Server.Services;

namespace ConferRecovery.Tests.Services;

public sealed class ApiTokenServiceTests
{
    private readonly JwtSettings _settings = new()
    {
        SecretKey = "test-secret-key-that-is-at-least-32-chars-long",
        Issuer = "test-issuer",
        Audience = "test-audience",
        ExpiryMinutes = 60
    };

    private readonly Member _member = new()
    {
        Id = "507f1f77bcf86cd799439011",
        DisplayName = "Jane Doe",
        Email = "jane@example.com",
        PasswordHash = "irrelevant",
        ChapterId = "507f1f77bcf86cd799439012",
        Role = MemberRole.Host,
        Status = MemberStatus.Active
    };

    private ApiTokenService Sut() => new(_settings);

    [Fact]
    public void Issue_ReturnsNonEmptyToken()
    {
        var result = Sut().Issue(_member);
        Assert.False(string.IsNullOrWhiteSpace(result.Value));
    }

    [Fact]
    public void Issue_TokenContainsSubClaim()
    {
        var result = Sut().Issue(_member);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Value);
        Assert.Equal(_member.Id, jwt.Subject);
    }

    [Fact]
    public void Issue_TokenContainsCorrectIssuerAndAudience()
    {
        var result = Sut().Issue(_member);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Value);
        Assert.Equal(_settings.Issuer, jwt.Issuer);
        Assert.Contains(_settings.Audience, jwt.Audiences);
    }

    [Fact]
    public void Issue_TokenContainsRoleClaim()
    {
        var result = Sut().Issue(_member);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Value);
        var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        Assert.Equal("Host", role);
    }

    [Fact]
    public void Issue_TokenExpiresAtConfiguredTime()
    {
        var before = DateTime.UtcNow;
        var result = Sut().Issue(_member);
        var after = DateTime.UtcNow;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Value);
        Assert.InRange(jwt.ValidTo,
            before.AddMinutes(_settings.ExpiryMinutes - 1),
            after.AddMinutes(_settings.ExpiryMinutes + 1));
    }

    [Fact]
    public void Issue_TokenPassesValidation()
    {
        var result = Sut().Issue(_member);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(result.Value, validationParams, out _);
        Assert.NotNull(principal);
    }
}
