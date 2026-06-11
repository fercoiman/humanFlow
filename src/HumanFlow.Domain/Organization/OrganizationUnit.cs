using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Organization;

public sealed class OrganizationUnit : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid? ParentId { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
}
