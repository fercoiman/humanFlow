using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class Employee : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string EmployeeNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials => $"{FirstName?.FirstOrDefault()}{LastName?.FirstOrDefault()}".ToUpperInvariant();

    // Contacto corporativo
    public string? Email { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }

    // Datos personales
    public DateOnly? BirthDate { get; set; }
    public string? NationalId { get; set; }
    public string? TaxId { get; set; }
    public MaritalStatus? MaritalStatus { get; set; }
    public string? Nationality { get; set; }

    // Domicilio
    public string? AddressStreet { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressState { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; }

    // Domicilio — referencias a datos maestros de geografía
    public Guid? CountryId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? LocalityId { get; set; }

    // Lugar de nacimiento — referencias a datos maestros de geografía
    public Guid? BirthCountryId { get; set; }
    public Guid? BirthCityId { get; set; }
    public Guid? BirthLocalityId { get; set; }

    // Estado laboral
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Draft;
    public DateOnly? HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }

    // Puesto actual (desnormalizado desde PositionHistory)
    public Guid? OrganizationUnitId { get; set; }
    public Guid? JobPositionId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
}
