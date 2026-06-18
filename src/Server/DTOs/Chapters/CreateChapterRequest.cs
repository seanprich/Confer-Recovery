using System.ComponentModel.DataAnnotations;

namespace SPQC.Confer.SelfHosted.Server.DTOs.Chapters;

public sealed record CreateChapterRequest(
    [Required, StringLength(120)] string Name,
    [Required, Url] string SfuUrl,
    [Required] string LiveKitApiKey,
    [Required] string LiveKitApiSecret);
