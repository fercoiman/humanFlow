using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Geography;

public sealed class Country : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public string? IsoCode { get; set; }
    public bool IsActive { get; set; } = true;
}
