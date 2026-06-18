using System.ComponentModel.DataAnnotations;

namespace SPQC.Confer.SelfHosted.Server.DTOs.Members;

public sealed record UpdateMemberStatusRequest([Required] string Status);

public sealed record UpdateMemberRoleRequest([Required] string Role);

public sealed record AcknowledgeConsentRequest([Required] string ConsentVersion);
