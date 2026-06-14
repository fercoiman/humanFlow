using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HumanFlow.Domain.Contacts;
using HumanFlow.Domain.Employees;
using HumanFlow.Domain.Organization;
using HumanFlow.Domain.Security;
using HumanFlow.Domain.Tenancy;

namespace HumanFlow.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<PermissionDefinition> PermissionDefinitions => Set<PermissionDefinition>();
    public DbSet<Role> HumanFlowRoles => Set<Role>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<EmployeeEvent> EmployeeEvents => Set<EmployeeEvent>();
    public DbSet<PositionHistory> PositionHistories => Set<PositionHistory>();
    public DbSet<EmployeeContact> EmployeeContacts => Set<EmployeeContact>();
    public DbSet<TerminationRecord> TerminationRecords => Set<TerminationRecord>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.Slug).HasMaxLength(100);
            entity.Property(x => x.PrimaryDomain).HasMaxLength(255);
        });

        builder.Entity<PermissionDefinition>(entity =>
        {
            entity.HasIndex(x => x.Key).IsUnique();
            entity.Property(x => x.Key).HasMaxLength(150);
            entity.Property(x => x.DisplayName).HasMaxLength(200);
            entity.Property(x => x.Module).HasMaxLength(100);
        });

        builder.Entity<Role>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        builder.Entity<Employee>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.EmployeeNumber }).IsUnique();
            entity.Property(x => x.EmployeeNumber).HasMaxLength(50);
            entity.Property(x => x.FirstName).HasMaxLength(150);
            entity.Property(x => x.LastName).HasMaxLength(150);
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.PersonalEmail).HasMaxLength(255);
            entity.Property(x => x.Phone).HasMaxLength(80);
            entity.Property(x => x.NationalId).HasMaxLength(50);
            entity.Property(x => x.TaxId).HasMaxLength(50);
            entity.Property(x => x.Nationality).HasMaxLength(100);
            entity.Property(x => x.AddressStreet).HasMaxLength(300);
            entity.Property(x => x.AddressCity).HasMaxLength(150);
            entity.Property(x => x.AddressState).HasMaxLength(150);
            entity.Property(x => x.AddressPostalCode).HasMaxLength(20);
            entity.Property(x => x.AddressCountry).HasMaxLength(100);
            entity.Ignore(x => x.FullName);
            entity.Ignore(x => x.Initials);
        });

        builder.Entity<PositionHistory>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.EmployeeId });
            entity.Property(x => x.Notes).HasMaxLength(1000);
        });

        builder.Entity<EmployeeContact>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.EmployeeId });
            entity.Property(x => x.FullName).HasMaxLength(200);
            entity.Property(x => x.Phone).HasMaxLength(80);
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.Notes).HasMaxLength(500);
        });

        builder.Entity<TerminationRecord>(entity =>
        {
            entity.HasIndex(x => x.EmployeeId).IsUnique();
            entity.Property(x => x.Reason).HasMaxLength(1000);
            entity.Property(x => x.ExitInterviewNotes).HasMaxLength(2000);
        });

        builder.Entity<Contact>(entity =>
        {
            entity.Property(x => x.DisplayName).HasMaxLength(200);
            entity.Property(x => x.OrganizationName).HasMaxLength(200);
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.Phone).HasMaxLength(80);
        });

        builder.Entity<PerformanceReview>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.EmployeeId });
            entity.HasIndex(x => new { x.EmployeeId, x.PeriodYear, x.PeriodType, x.PeriodNumber }).IsUnique();
            entity.Property(x => x.StrengthsNotes).HasMaxLength(2000);
            entity.Property(x => x.ImprovementNotes).HasMaxLength(2000);
            entity.Property(x => x.GoalsNotes).HasMaxLength(2000);
            entity.Property(x => x.AcknowledgementNotes).HasMaxLength(1000);
            entity.Ignore(x => x.PeriodLabel);
        });
    }
}
