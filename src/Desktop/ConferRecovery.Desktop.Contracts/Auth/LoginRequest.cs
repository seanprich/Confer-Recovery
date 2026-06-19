using System.ComponentModel.DataAnnotations;

namespace ConferRecovery.Desktop.Contracts.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password);