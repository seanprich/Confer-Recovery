using System.ComponentModel.DataAnnotations;

namespace ConferRecovery.Server.DTOs.Members;

public sealed record CreateMemberRequest(
    [Required, StringLength(80)] string DisplayName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(12)] string Password,
    [Required] string ChapterId,
    string Role = "Listener");
