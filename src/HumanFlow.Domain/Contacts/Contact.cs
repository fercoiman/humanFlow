using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Contacts;

public sealed class Contact : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public ContactKind Kind { get; set; }
    public ContactType Type { get; set; }
    public required string DisplayName { get; set; }
    public string? OrganizationName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
