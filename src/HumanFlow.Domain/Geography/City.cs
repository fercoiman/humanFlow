using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Geography;

public sealed class City : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid CountryId { get; set; }
    public required string Name { get; set; }
    public string? StateName { get; set; }
    public bool IsActive { get; set; } = true;

    public Country? Country { get; set; }
}
