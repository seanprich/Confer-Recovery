using System.ComponentModel.DataAnnotations;

namespace ConferRecovery.Server.DTOs.Rooms;

public sealed record CreateRoomRequest(
    [Required, StringLength(120)] string Name,
    [Required] string ChapterId,
    DateTime? ScheduledAt = null);
