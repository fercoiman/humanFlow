using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class PerformanceReview : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public ReviewPeriodType PeriodType { get; set; } = ReviewPeriodType.Annual;
    public int PeriodYear { get; set; }
    public int PeriodNumber { get; set; } = 1;
    public DateOnly ReviewDate { get; set; }
    public Guid? ReviewerEmployeeId { get; set; }
    public ReviewRating OverallRating { get; set; } = ReviewRating.MeetsExpectations;
    public string? StrengthsNotes { get; set; }
    public string? ImprovementNotes { get; set; }
    public string? GoalsNotes { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Draft;
    public DateOnly? AcknowledgedDate { get; set; }
    public string? AcknowledgementNotes { get; set; }

    public string PeriodLabel => PeriodType switch
    {
        ReviewPeriodType.SemiAnnual => PeriodNumber == 1
            ? $"1er Semestre {PeriodYear}"
            : $"2do Semestre {PeriodYear}",
        ReviewPeriodType.Quarterly => PeriodNumber switch
        {
            1 => $"1er Trimestre {PeriodYear}",
            2 => $"2do Trimestre {PeriodYear}",
            3 => $"3er Trimestre {PeriodYear}",
            _ => $"4to Trimestre {PeriodYear}"
        },
        _ => $"Anual {PeriodYear}"
    };
}
