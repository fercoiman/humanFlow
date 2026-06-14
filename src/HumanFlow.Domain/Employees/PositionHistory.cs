using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class PositionHistory : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid? JobPositionId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public PositionChangeReason Reason { get; set; } = PositionChangeReason.Hire;
    public string? Notes { get; set; }
}
