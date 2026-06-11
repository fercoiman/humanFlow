using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Tenancy;

public sealed class Tenant : AuditableEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public TenantStorageMode StorageMode { get; set; } = TenantStorageMode.SharedDatabase;
    public string? PrimaryDomain { get; set; }
}
