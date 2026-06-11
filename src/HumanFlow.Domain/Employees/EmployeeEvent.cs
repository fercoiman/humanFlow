using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class EmployeeEvent : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public required string EventType { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
