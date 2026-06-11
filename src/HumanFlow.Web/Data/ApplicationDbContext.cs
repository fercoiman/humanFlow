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
            entity.Ignore(x => x.FullName);
        });

        builder.Entity<Contact>(entity =>
        {
            entity.Property(x => x.DisplayName).HasMaxLength(200);
            entity.Property(x => x.OrganizationName).HasMaxLength(200);
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.Phone).HasMaxLength(80);
        });
    }
}
