using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Geography;

public sealed class Locality : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid CityId { get; set; }
    public required string Name { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; } = true;

    public City? City { get; set; }
}
