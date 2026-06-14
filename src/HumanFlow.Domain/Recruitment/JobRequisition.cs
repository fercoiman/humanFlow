using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Recruitment;

public sealed class JobRequisition : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string RequisitionNumber { get; set; }
    public required string Title { get; set; }
    public Guid? JobPositionId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public Guid? RequestedByEmployeeId { get; set; }
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Open;
    public RequisitionPriority Priority { get; set; } = RequisitionPriority.Normal;
    public int VacanciesCount { get; set; } = 1;
    public DateOnly? OpenDate { get; set; }
    public DateOnly? TargetFillDate { get; set; }
    public DateOnly? ClosedDate { get; set; }
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public bool IsRemote { get; set; }
    public string? Notes { get; set; }
}
