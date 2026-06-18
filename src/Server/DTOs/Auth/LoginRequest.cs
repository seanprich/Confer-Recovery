using System.ComponentModel.DataAnnotations;

namespace SPQC.Confer.SelfHosted.Server.DTOs.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);
