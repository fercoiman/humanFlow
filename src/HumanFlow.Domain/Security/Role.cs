using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Security;

public sealed class Role : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public bool IsSystemRole { get; set; }
}
