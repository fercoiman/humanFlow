using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Recruitment;

public sealed class JobApplication : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid CandidateId { get; set; }
    public Guid JobRequisitionId { get; set; }
    public DateOnly ApplicationDate { get; set; }
    public JobApplicationStatus Status { get; set; } = JobApplicationStatus.New;
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public Guid? HiredEmployeeId { get; set; }
}
