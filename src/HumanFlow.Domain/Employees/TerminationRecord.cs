using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class TerminationRecord : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly ExitDate { get; set; }
    public ExitType ExitType { get; set; }
    public string? Reason { get; set; }
    public string? ExitInterviewNotes { get; set; }
    public bool IsEligibleForRehire { get; set; } = true;
}
