using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class Employee : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string EmployeeNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? NationalId { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Draft;
    public DateOnly? HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public Guid? JobPositionId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
}
