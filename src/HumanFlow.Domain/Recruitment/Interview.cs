using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Recruitment;

public sealed class Interview : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid JobApplicationId { get; set; }
    public InterviewType Type { get; set; } = InterviewType.Phone;
    public DateOnly? ScheduledDate { get; set; }
    public Guid? InterviewerEmployeeId { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;
    public int? Rating { get; set; }
    public string? Feedback { get; set; }
    public string? Notes { get; set; }
}
