using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Organization;

public sealed class JobPosition : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string Title { get; set; }
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}
