using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Recruitment;

public sealed class Candidate : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? LinkedInUrl { get; set; }
    public CandidateSource Source { get; set; } = CandidateSource.Direct;
    public Guid? ReferredByEmployeeId { get; set; }
    public string? Notes { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials => $"{FirstName?.FirstOrDefault()}{LastName?.FirstOrDefault()}".ToUpperInvariant();
}
