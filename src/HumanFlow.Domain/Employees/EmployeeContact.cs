using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class EmployeeContact : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public required string FullName { get; set; }
    public ContactRelationship Relationship { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsEmergencyContact { get; set; }
    public bool IsPrimaryEmergencyContact { get; set; }
    public string? Notes { get; set; }
}
