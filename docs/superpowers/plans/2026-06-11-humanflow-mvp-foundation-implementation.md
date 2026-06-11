# HumanFlow MVP Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first working HumanFlow foundation: Blazor Web App, SQL Server-ready persistence, hybrid multi-tenancy primitives, identity, roles, permissions, employee files, contacts, master data, and mandatory tenant-isolation tests.

**Architecture:** Start as a modular monolith with clear project boundaries: Web, Domain, Application, Infrastructure, and Tests. The first implementation uses one shared database with tenant-scoped records while preserving a tenant catalog model that can later route selected tenants to dedicated databases.

**Tech Stack:** .NET 10, C#, Blazor Web App with Interactive Server, ASP.NET Core Identity, Entity Framework Core SQL Server provider, xUnit.

---

## File Structure

Create this structure:

```text
HumanFlow.slnx
global.json
src/
  HumanFlow.Domain/
    Common/
      AuditableEntity.cs
      Entity.cs
      ITenantScoped.cs
    Tenancy/
      Tenant.cs
      TenantDomain.cs
      TenantStorageMode.cs
      TenantStatus.cs
    Security/
      PermissionDefinition.cs
      Role.cs
      RolePermission.cs
      UserTenantMembership.cs
      PlatformRole.cs
    Organization/
      OrganizationUnit.cs
      JobPosition.cs
    Employees/
      Employee.cs
      EmployeeDocument.cs
      EmployeeEvent.cs
      EmployeeStatus.cs
      EmployeeContact.cs
    Contacts/
      Contact.cs
      ContactKind.cs
      ContactType.cs
  HumanFlow.Application/
    Tenancy/
      CurrentTenant.cs
      ICurrentTenant.cs
      ITenantResolver.cs
      TenantResolutionResult.cs
    Security/
      HumanFlowPermissions.cs
      IPermissionChecker.cs
    Employees/
      EmployeeListItem.cs
      IEmployeeService.cs
    Contacts/
      ContactListItem.cs
      IContactService.cs
  HumanFlow.Infrastructure/
    Data/
      HumanFlowDbContext.cs
      DbSeeder.cs
    Identity/
      ApplicationUser.cs
    Security/
      PermissionChecker.cs
    Employees/
      EmployeeService.cs
    Contacts/
      ContactService.cs
    Tenancy/
      HeaderOrQueryTenantResolver.cs
      TenantMiddleware.cs
  HumanFlow.Web/
    Components/
      Layout/
      Pages/
        Admin/
          Tenants.razor
          Roles.razor
          Users.razor
        Employees/
          Employees.razor
          EmployeeDetails.razor
        Contacts/
          Contacts.razor
        MasterData/
          OrganizationUnits.razor
          JobPositions.razor
    Program.cs
    appsettings.json
tests/
  HumanFlow.Tests/
    Tenancy/
      TenantIsolationTests.cs
    Security/
      PermissionCheckerTests.cs
    Employees/
      EmployeeServiceTests.cs
```

Responsibilities:

- `Domain`: entity model and domain enums only; no EF Core, no Blazor, no infrastructure.
- `Application`: service contracts, DTOs, permission constants, and use-case services.
- `Infrastructure`: EF Core, Identity user, tenant middleware, seed data, and permission implementation.
- `Web`: Blazor UI, authentication shell, dependency injection, and app startup.
- `Tests`: risk-focused tests for tenant isolation, permissions, and core CRUD flows.

---

## Task 1: Initialize Solution And Projects

**Files:**
- Create: `global.json`
- Create: `HumanFlow.slnx`
- Create: `src/HumanFlow.Web/HumanFlow.Web.csproj`
- Create: `src/HumanFlow.Domain/HumanFlow.Domain.csproj`
- Create: `src/HumanFlow.Application/HumanFlow.Application.csproj`
- Create: `src/HumanFlow.Infrastructure/HumanFlow.Infrastructure.csproj`
- Create: `tests/HumanFlow.Tests/HumanFlow.Tests.csproj`

- [ ] **Step 1: Initialize git repository**

Run:

```bash
git init
```

Expected: repository initialized.

- [ ] **Step 2: Pin local SDK**

Create `global.json`:

```json
{
  "sdk": {
    "version": "10.0.300",
    "rollForward": "latestFeature"
  }
}
```

- [ ] **Step 3: Scaffold solution and projects**

Run:

```bash
dotnet new sln -n HumanFlow
dotnet new classlib -n HumanFlow.Domain -o src/HumanFlow.Domain -f net10.0
dotnet new classlib -n HumanFlow.Application -o src/HumanFlow.Application -f net10.0
dotnet new classlib -n HumanFlow.Infrastructure -o src/HumanFlow.Infrastructure -f net10.0
dotnet new blazor -n HumanFlow.Web -o src/HumanFlow.Web -f net10.0 --auth Individual --interactivity Server
dotnet new xunit -n HumanFlow.Tests -o tests/HumanFlow.Tests -f net10.0
```

Expected: all projects created.

- [ ] **Step 4: Add projects to solution**

Run:

```bash
dotnet sln HumanFlow.slnx add src/HumanFlow.Domain/HumanFlow.Domain.csproj
dotnet sln HumanFlow.slnx add src/HumanFlow.Application/HumanFlow.Application.csproj
dotnet sln HumanFlow.slnx add src/HumanFlow.Infrastructure/HumanFlow.Infrastructure.csproj
dotnet sln HumanFlow.slnx add src/HumanFlow.Web/HumanFlow.Web.csproj
dotnet sln HumanFlow.slnx add tests/HumanFlow.Tests/HumanFlow.Tests.csproj
```

Expected: all projects added.

- [ ] **Step 5: Add project references**

Run:

```bash
dotnet add src/HumanFlow.Application/HumanFlow.Application.csproj reference src/HumanFlow.Domain/HumanFlow.Domain.csproj
dotnet add src/HumanFlow.Infrastructure/HumanFlow.Infrastructure.csproj reference src/HumanFlow.Domain/HumanFlow.Domain.csproj
dotnet add src/HumanFlow.Infrastructure/HumanFlow.Infrastructure.csproj reference src/HumanFlow.Application/HumanFlow.Application.csproj
dotnet add src/HumanFlow.Web/HumanFlow.Web.csproj reference src/HumanFlow.Application/HumanFlow.Application.csproj
dotnet add src/HumanFlow.Web/HumanFlow.Web.csproj reference src/HumanFlow.Infrastructure/HumanFlow.Infrastructure.csproj
dotnet add tests/HumanFlow.Tests/HumanFlow.Tests.csproj reference src/HumanFlow.Domain/HumanFlow.Domain.csproj
dotnet add tests/HumanFlow.Tests/HumanFlow.Tests.csproj reference src/HumanFlow.Application/HumanFlow.Application.csproj
dotnet add tests/HumanFlow.Tests/HumanFlow.Tests.csproj reference src/HumanFlow.Infrastructure/HumanFlow.Infrastructure.csproj
```

Expected: references added.

- [ ] **Step 6: Add packages**

Run:

```bash
dotnet add src/HumanFlow.Infrastructure/HumanFlow.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/HumanFlow.Infrastructure/HumanFlow.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/HumanFlow.Infrastructure/HumanFlow.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/HumanFlow.Web/HumanFlow.Web.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add tests/HumanFlow.Tests/HumanFlow.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory
```

Expected: packages restored.

- [ ] **Step 7: Build**

Run:

```bash
dotnet build HumanFlow.slnx
```

Expected: build succeeds.

- [ ] **Step 8: Commit**

Run:

```bash
git add .
git commit -m "chore: initialize HumanFlow solution"
```

Expected: commit created.

---

## Task 2: Create Domain Foundation

**Files:**
- Create: `src/HumanFlow.Domain/Common/Entity.cs`
- Create: `src/HumanFlow.Domain/Common/AuditableEntity.cs`
- Create: `src/HumanFlow.Domain/Common/ITenantScoped.cs`
- Create: `src/HumanFlow.Domain/Tenancy/TenantStorageMode.cs`
- Create: `src/HumanFlow.Domain/Tenancy/TenantStatus.cs`
- Create: `src/HumanFlow.Domain/Tenancy/Tenant.cs`
- Create: `src/HumanFlow.Domain/Tenancy/TenantDomain.cs`

- [ ] **Step 1: Remove template class**

Run:

```bash
rm src/HumanFlow.Domain/Class1.cs
```

Expected: template file removed.

- [ ] **Step 2: Add base entity types**

Create `src/HumanFlow.Domain/Common/Entity.cs`:

```csharp
namespace HumanFlow.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
```

Create `src/HumanFlow.Domain/Common/AuditableEntity.cs`:

```csharp
namespace HumanFlow.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedByUserId { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; }
}
```

Create `src/HumanFlow.Domain/Common/ITenantScoped.cs`:

```csharp
namespace HumanFlow.Domain.Common;

public interface ITenantScoped
{
    Guid TenantId { get; set; }
}
```

- [ ] **Step 3: Add tenancy domain types**

Create `src/HumanFlow.Domain/Tenancy/TenantStorageMode.cs`:

```csharp
namespace HumanFlow.Domain.Tenancy;

public enum TenantStorageMode
{
    SharedDatabase = 1,
    DedicatedDatabase = 2
}
```

Create `src/HumanFlow.Domain/Tenancy/TenantStatus.cs`:

```csharp
namespace HumanFlow.Domain.Tenancy;

public enum TenantStatus
{
    Provisioning = 1,
    Active = 2,
    Suspended = 3,
    Archived = 4
}
```

Create `src/HumanFlow.Domain/Tenancy/Tenant.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Tenancy;

public sealed class Tenant : AuditableEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public TenantStorageMode StorageMode { get; set; } = TenantStorageMode.SharedDatabase;
    public string? DedicatedConnectionStringName { get; set; }
    public ICollection<TenantDomain> Domains { get; set; } = new List<TenantDomain>();
}
```

Create `src/HumanFlow.Domain/Tenancy/TenantDomain.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Tenancy;

public sealed class TenantDomain : Entity
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public required string HostName { get; set; }
    public bool IsPrimary { get; set; }
}
```

- [ ] **Step 4: Build**

Run:

```bash
dotnet build HumanFlow.slnx
```

Expected: build succeeds.

- [ ] **Step 5: Commit**

Run:

```bash
git add src/HumanFlow.Domain
git commit -m "feat: add domain tenancy foundation"
```

Expected: commit created.

---

## Task 3: Add Security, Organization, Employee, And Contact Domain

**Files:**
- Create: `src/HumanFlow.Domain/Security/*.cs`
- Create: `src/HumanFlow.Domain/Organization/*.cs`
- Create: `src/HumanFlow.Domain/Employees/*.cs`
- Create: `src/HumanFlow.Domain/Contacts/*.cs`

- [ ] **Step 1: Add security entities**

Create `src/HumanFlow.Domain/Security/PlatformRole.cs`:

```csharp
namespace HumanFlow.Domain.Security;

public enum PlatformRole
{
    None = 0,
    PlatformAdministrator = 1,
    PlatformSupportOperator = 2
}
```

Create `src/HumanFlow.Domain/Security/PermissionDefinition.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Security;

public sealed class PermissionDefinition : Entity
{
    public required string Key { get; set; }
    public required string DisplayName { get; set; }
    public required string Module { get; set; }
}
```

Create `src/HumanFlow.Domain/Security/Role.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Security;

public sealed class Role : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string Name { get; set; }
    public bool IsSystemRole { get; set; }
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
}
```

Create `src/HumanFlow.Domain/Security/RolePermission.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Security;

public sealed class RolePermission : Entity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionDefinitionId { get; set; }
    public PermissionDefinition PermissionDefinition { get; set; } = null!;
}
```

Create `src/HumanFlow.Domain/Security/UserTenantMembership.cs`:

```csharp
using HumanFlow.Domain.Common;
using HumanFlow.Domain.Tenancy;

namespace HumanFlow.Domain.Security;

public sealed class UserTenantMembership : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
```

- [ ] **Step 2: Add organization entities**

Create `src/HumanFlow.Domain/Organization/OrganizationUnit.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Organization;

public sealed class OrganizationUnit : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid? ParentId { get; set; }
    public OrganizationUnit? Parent { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
}
```

Create `src/HumanFlow.Domain/Organization/JobPosition.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Organization;

public sealed class JobPosition : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string Title { get; set; }
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
}
```

- [ ] **Step 3: Add contact entities**

Create `src/HumanFlow.Domain/Contacts/ContactKind.cs`:

```csharp
namespace HumanFlow.Domain.Contacts;

public enum ContactKind
{
    Internal = 1,
    External = 2
}
```

Create `src/HumanFlow.Domain/Contacts/ContactType.cs`:

```csharp
namespace HumanFlow.Domain.Contacts;

public enum ContactType
{
    Emergency = 1,
    Provider = 2,
    Consultant = 3,
    Legal = 4,
    Medical = 5,
    Reference = 6,
    Operational = 7
}
```

Create `src/HumanFlow.Domain/Contacts/Contact.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Contacts;

public sealed class Contact : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public ContactKind Kind { get; set; }
    public ContactType Type { get; set; }
    public required string DisplayName { get; set; }
    public string? OrganizationName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
```

- [ ] **Step 4: Add employee entities**

Create `src/HumanFlow.Domain/Employees/EmployeeStatus.cs`:

```csharp
namespace HumanFlow.Domain.Employees;

public enum EmployeeStatus
{
    Draft = 1,
    Active = 2,
    OnLeave = 3,
    Terminated = 4
}
```

Create `src/HumanFlow.Domain/Employees/Employee.cs`:

```csharp
using HumanFlow.Domain.Common;
using HumanFlow.Domain.Organization;

namespace HumanFlow.Domain.Employees;

public sealed class Employee : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public required string EmployeeNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public string? PersonalEmail { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? NationalId { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Draft;
    public DateOnly? HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public OrganizationUnit? OrganizationUnit { get; set; }
    public Guid? JobPositionId { get; set; }
    public JobPosition? JobPosition { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public Employee? ManagerEmployee { get; set; }
    public ICollection<EmployeeContact> Contacts { get; set; } = new List<EmployeeContact>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeEvent> Events { get; set; } = new List<EmployeeEvent>();
}
```

Create `src/HumanFlow.Domain/Employees/EmployeeContact.cs`:

```csharp
using HumanFlow.Domain.Common;
using HumanFlow.Domain.Contacts;

namespace HumanFlow.Domain.Employees;

public sealed class EmployeeContact : Entity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid ContactId { get; set; }
    public Contact Contact { get; set; } = null!;
    public required string Relationship { get; set; }
}
```

Create `src/HumanFlow.Domain/Employees/EmployeeDocument.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class EmployeeDocument : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public required string DocumentType { get; set; }
    public required string FileName { get; set; }
    public required string StorageKey { get; set; }
    public string? ContentType { get; set; }
    public long? SizeInBytes { get; set; }
}
```

Create `src/HumanFlow.Domain/Employees/EmployeeEvent.cs`:

```csharp
using HumanFlow.Domain.Common;

namespace HumanFlow.Domain.Employees;

public sealed class EmployeeEvent : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public required string EventType { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
```

- [ ] **Step 5: Build**

Run:

```bash
dotnet build HumanFlow.slnx
```

Expected: build succeeds.

- [ ] **Step 6: Commit**

Run:

```bash
git add src/HumanFlow.Domain
git commit -m "feat: add HR foundation domain model"
```

Expected: commit created.

---

## Task 4: Add Current Tenant Contracts And Permission Constants

**Files:**
- Create: `src/HumanFlow.Application/Tenancy/ICurrentTenant.cs`
- Create: `src/HumanFlow.Application/Tenancy/CurrentTenant.cs`
- Create: `src/HumanFlow.Application/Tenancy/ITenantResolver.cs`
- Create: `src/HumanFlow.Application/Tenancy/TenantResolutionResult.cs`
- Create: `src/HumanFlow.Application/Security/HumanFlowPermissions.cs`
- Create: `src/HumanFlow.Application/Security/IPermissionChecker.cs`

- [ ] **Step 1: Remove template class**

Run:

```bash
rm src/HumanFlow.Application/Class1.cs
```

Expected: template file removed.

- [ ] **Step 2: Add tenant contracts**

Create `src/HumanFlow.Application/Tenancy/ICurrentTenant.cs`:

```csharp
namespace HumanFlow.Application.Tenancy;

public interface ICurrentTenant
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
    bool IsAvailable { get; }
    void Set(Guid tenantId, string tenantSlug);
    void Clear();
}
```

Create `src/HumanFlow.Application/Tenancy/CurrentTenant.cs`:

```csharp
namespace HumanFlow.Application.Tenancy;

public sealed class CurrentTenant : ICurrentTenant
{
    public Guid? TenantId { get; private set; }
    public string? TenantSlug { get; private set; }
    public bool IsAvailable => TenantId.HasValue;

    public void Set(Guid tenantId, string tenantSlug)
    {
        TenantId = tenantId;
        TenantSlug = tenantSlug;
    }

    public void Clear()
    {
        TenantId = null;
        TenantSlug = null;
    }
}
```

Create `src/HumanFlow.Application/Tenancy/TenantResolutionResult.cs`:

```csharp
namespace HumanFlow.Application.Tenancy;

public sealed record TenantResolutionResult(Guid TenantId, string TenantSlug);
```

Create `src/HumanFlow.Application/Tenancy/ITenantResolver.cs`:

```csharp
namespace HumanFlow.Application.Tenancy;

public interface ITenantResolver
{
    Task<TenantResolutionResult?> ResolveAsync(CancellationToken cancellationToken);
}
```

- [ ] **Step 3: Add permission constants**

Create `src/HumanFlow.Application/Security/HumanFlowPermissions.cs`:

```csharp
namespace HumanFlow.Application.Security;

public static class HumanFlowPermissions
{
    public const string PlatformTenantsManage = "platform.tenants.manage";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";
    public const string TenantSettingsManage = "tenant.settings.manage";
    public const string EmployeesRead = "employees.read";
    public const string EmployeesCreate = "employees.create";
    public const string EmployeesUpdate = "employees.update";
    public const string EmployeeDocumentsManage = "employees.documents.manage";
    public const string ContactsRead = "contacts.read";
    public const string ContactsManage = "contacts.manage";
    public const string MasterDataManage = "masterdata.manage";

    public static readonly string[] All =
    [
        PlatformTenantsManage,
        UsersManage,
        RolesManage,
        TenantSettingsManage,
        EmployeesRead,
        EmployeesCreate,
        EmployeesUpdate,
        EmployeeDocumentsManage,
        ContactsRead,
        ContactsManage,
        MasterDataManage
    ];
}
```

Create `src/HumanFlow.Application/Security/IPermissionChecker.cs`:

```csharp
using System.Security.Claims;

namespace HumanFlow.Application.Security;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission, CancellationToken cancellationToken);
}
```

- [ ] **Step 4: Build and commit**

Run:

```bash
dotnet build HumanFlow.slnx
git add src/HumanFlow.Application
git commit -m "feat: add tenancy and permission application contracts"
```

Expected: build succeeds and commit created.

---

## Task 5: Add EF Core DbContext With Tenant Filters

**Files:**
- Create: `src/HumanFlow.Infrastructure/Identity/ApplicationUser.cs`
- Create: `src/HumanFlow.Infrastructure/Data/HumanFlowDbContext.cs`
- Modify: `src/HumanFlow.Web/Program.cs`
- Modify: `src/HumanFlow.Web/appsettings.json`

- [ ] **Step 1: Remove template class**

Run:

```bash
rm src/HumanFlow.Infrastructure/Class1.cs
```

Expected: template file removed.

- [ ] **Step 2: Add application user**

Create `src/HumanFlow.Infrastructure/Identity/ApplicationUser.cs`:

```csharp
using HumanFlow.Domain.Security;
using Microsoft.AspNetCore.Identity;

namespace HumanFlow.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public PlatformRole PlatformRole { get; set; } = PlatformRole.None;
}
```

- [ ] **Step 3: Add DbContext**

Create `src/HumanFlow.Infrastructure/Data/HumanFlowDbContext.cs`:

```csharp
using HumanFlow.Application.Tenancy;
using HumanFlow.Domain.Common;
using HumanFlow.Domain.Contacts;
using HumanFlow.Domain.Employees;
using HumanFlow.Domain.Organization;
using HumanFlow.Domain.Security;
using HumanFlow.Domain.Tenancy;
using HumanFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HumanFlow.Infrastructure.Data;

public sealed class HumanFlowDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private readonly ICurrentTenant _currentTenant;

    public HumanFlowDbContext(DbContextOptions<HumanFlowDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantDomain> TenantDomains => Set<TenantDomain>();
    public DbSet<PermissionDefinition> PermissionDefinitions => Set<PermissionDefinition>();
    public DbSet<Role> HumanFlowRoles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserTenantMembership> UserTenantMemberships => Set<UserTenantMembership>();
    public DbSet<OrganizationUnit> OrganizationUnits => Set<OrganizationUnit>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeContact> EmployeeContacts => Set<EmployeeContact>();
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
        });

        builder.Entity<TenantDomain>(entity =>
        {
            entity.HasIndex(x => x.HostName).IsUnique();
            entity.Property(x => x.HostName).HasMaxLength(255);
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
        });

        builder.Entity<Employee>(entity =>
        {
            entity.HasIndex(x => new { x.TenantId, x.EmployeeNumber }).IsUnique();
            entity.Property(x => x.EmployeeNumber).HasMaxLength(50);
            entity.Property(x => x.FirstName).HasMaxLength(150);
            entity.Property(x => x.LastName).HasMaxLength(150);
            entity.Property(x => x.Email).HasMaxLength(255);
        });

        builder.Entity<Contact>(entity =>
        {
            entity.Property(x => x.DisplayName).HasMaxLength(200);
            entity.Property(x => x.Email).HasMaxLength(255);
            entity.Property(x => x.Phone).HasMaxLength(80);
        });

        ApplyTenantFilters(builder);
    }

    private void ApplyTenantFilters(ModelBuilder builder)
    {
        builder.Entity<Role>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
        builder.Entity<RolePermission>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
        builder.Entity<UserTenantMembership>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
        builder.Entity<OrganizationUnit>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
        builder.Entity<JobPosition>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
        builder.Entity<Employee>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
        builder.Entity<EmployeeContact>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
        builder.Entity<EmployeeDocument>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
        builder.Entity<EmployeeEvent>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
        builder.Entity<Contact>().HasQueryFilter(x => !_currentTenant.TenantId.HasValue || x.TenantId == _currentTenant.TenantId);
    }
}
```

- [ ] **Step 4: Wire services in Program.cs**

Modify `src/HumanFlow.Web/Program.cs` so the service registration uses `HumanFlowDbContext`, `ApplicationUser`, and SQL Server:

```csharp
using HumanFlow.Application.Tenancy;
using HumanFlow.Infrastructure.Data;
using HumanFlow.Infrastructure.Identity;
using HumanFlow.Web.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();

builder.Services.AddDbContext<HumanFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 10;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<HumanFlowDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

- [ ] **Step 5: Set SQL Server connection string**

Modify `src/HumanFlow.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HumanFlow;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 6: Build**

Run:

```bash
dotnet build HumanFlow.slnx
```

Expected: build succeeds.

Run this check:

```bash
rg -n "ApplicationDbContext|IdentityUser<|IdentityUser\\b" src/HumanFlow.Web src/HumanFlow.Infrastructure
```

Expected: no references to template `ApplicationDbContext`; `ApplicationUser` is the only app user type used by startup and Identity.

- [ ] **Step 7: Commit**

Run:

```bash
git add src/HumanFlow.Infrastructure src/HumanFlow.Web
git commit -m "feat: add SQL Server identity db context"
```

Expected: commit created.

---

## Task 6: Add Tenant Isolation Tests

**Files:**
- Create: `tests/HumanFlow.Tests/Tenancy/TenantIsolationTests.cs`

- [ ] **Step 1: Create failing tenant isolation test**

Create `tests/HumanFlow.Tests/Tenancy/TenantIsolationTests.cs`:

```csharp
using HumanFlow.Application.Tenancy;
using HumanFlow.Domain.Employees;
using HumanFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HumanFlow.Tests.Tenancy;

public sealed class TenantIsolationTests
{
    [Fact]
    public async Task employees_query_only_returns_current_tenant_records()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var currentTenant = new CurrentTenant();
        currentTenant.Set(tenantA, "tenant-a");

        var options = new DbContextOptionsBuilder<HumanFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using (var seedContext = new HumanFlowDbContext(options, currentTenant))
        {
            seedContext.Employees.Add(new Employee
            {
                TenantId = tenantA,
                EmployeeNumber = "A-001",
                FirstName = "Ana",
                LastName = "Alvarez"
            });
            seedContext.Employees.Add(new Employee
            {
                TenantId = tenantB,
                EmployeeNumber = "B-001",
                FirstName = "Bruno",
                LastName = "Benitez"
            });
            await seedContext.SaveChangesAsync();
        }

        await using var queryContext = new HumanFlowDbContext(options, currentTenant);
        var employees = await queryContext.Employees.ToListAsync();

        Assert.Single(employees);
        Assert.Equal("A-001", employees[0].EmployeeNumber);
    }
}
```

- [ ] **Step 2: Run test**

Run:

```bash
dotnet test tests/HumanFlow.Tests/HumanFlow.Tests.csproj --filter employees_query_only_returns_current_tenant_records
```

Expected: PASS, proving the global query filter is active.

- [ ] **Step 3: Commit**

Run:

```bash
git add tests/HumanFlow.Tests
git commit -m "test: prove tenant isolation filter"
```

Expected: commit created.

---

## Task 7: Add Tenant Resolution Middleware

**Files:**
- Create: `src/HumanFlow.Infrastructure/Tenancy/HeaderOrQueryTenantResolver.cs`
- Create: `src/HumanFlow.Infrastructure/Tenancy/TenantMiddleware.cs`
- Modify: `src/HumanFlow.Web/Program.cs`
- Create: `tests/HumanFlow.Tests/Tenancy/TenantResolutionTests.cs`

- [ ] **Step 1: Add resolver test**

Create `tests/HumanFlow.Tests/Tenancy/TenantResolutionTests.cs`:

```csharp
using HumanFlow.Application.Tenancy;
using HumanFlow.Domain.Tenancy;
using HumanFlow.Infrastructure.Data;
using HumanFlow.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HumanFlow.Tests.Tenancy;

public sealed class TenantResolutionTests
{
    [Fact]
    public async Task resolver_uses_x_tenant_slug_header()
    {
        var currentTenant = new CurrentTenant();
        var options = new DbContextOptionsBuilder<HumanFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var tenant = new Tenant { Name = "Acme", Slug = "acme" };
        await using (var context = new HumanFlowDbContext(options, currentTenant))
        {
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tenant-Slug"] = "acme";
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        await using var queryContext = new HumanFlowDbContext(options, currentTenant);
        var resolver = new HeaderOrQueryTenantResolver(accessor, queryContext);

        var result = await resolver.ResolveAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(tenant.Id, result!.TenantId);
        Assert.Equal("acme", result.TenantSlug);
    }
}
```

- [ ] **Step 2: Run test and verify failure**

Run:

```bash
dotnet test tests/HumanFlow.Tests/HumanFlow.Tests.csproj --filter resolver_uses_x_tenant_slug_header
```

Expected: FAIL because `HeaderOrQueryTenantResolver` does not exist.

- [ ] **Step 3: Implement resolver and middleware**

Create `src/HumanFlow.Infrastructure/Tenancy/HeaderOrQueryTenantResolver.cs`:

```csharp
using HumanFlow.Application.Tenancy;
using HumanFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HumanFlow.Infrastructure.Tenancy;

public sealed class HeaderOrQueryTenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HumanFlowDbContext _dbContext;

    public HeaderOrQueryTenantResolver(IHttpContextAccessor httpContextAccessor, HumanFlowDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public async Task<TenantResolutionResult?> ResolveAsync(CancellationToken cancellationToken)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return null;
        }

        var slug = request.Headers["X-Tenant-Slug"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = request.Query["tenant"].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var tenant = await _dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

        return tenant is null ? null : new TenantResolutionResult(tenant.Id, tenant.Slug);
    }
}
```

Create `src/HumanFlow.Infrastructure/Tenancy/TenantMiddleware.cs`:

```csharp
using HumanFlow.Application.Tenancy;

namespace HumanFlow.Infrastructure.Tenancy;

public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver resolver, ICurrentTenant currentTenant)
    {
        var result = await resolver.ResolveAsync(context.RequestAborted);
        if (result is not null)
        {
            currentTenant.Set(result.TenantId, result.TenantSlug);
        }

        await _next(context);
    }
}
```

- [ ] **Step 4: Register resolver and middleware**

Modify `src/HumanFlow.Web/Program.cs`:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantResolver, HeaderOrQueryTenantResolver>();
```

Add middleware after authentication and before authorization:

```csharp
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();
```

Also add:

```csharp
using HumanFlow.Infrastructure.Tenancy;
```

- [ ] **Step 5: Run tests and build**

Run:

```bash
dotnet test HumanFlow.slnx
dotnet build HumanFlow.slnx
```

Expected: tests and build pass.

- [ ] **Step 6: Commit**

Run:

```bash
git add src/HumanFlow.Infrastructure src/HumanFlow.Web tests/HumanFlow.Tests
git commit -m "feat: resolve current tenant from request"
```

Expected: commit created.

---

## Task 8: Seed Tenants, Permissions, Roles, And Demo Data

**Files:**
- Create: `src/HumanFlow.Infrastructure/Data/DbSeeder.cs`
- Modify: `src/HumanFlow.Web/Program.cs`

- [ ] **Step 1: Add database seeder**

Create `src/HumanFlow.Infrastructure/Data/DbSeeder.cs`:

```csharp
using HumanFlow.Application.Security;
using HumanFlow.Domain.Contacts;
using HumanFlow.Domain.Employees;
using HumanFlow.Domain.Organization;
using HumanFlow.Domain.Security;
using HumanFlow.Domain.Tenancy;
using HumanFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HumanFlow.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HumanFlowDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        var tenant = await db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Slug == "demo");
        if (tenant is null)
        {
            tenant = new Tenant { Name = "Demo Company", Slug = "demo" };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();
        }

        foreach (var permission in HumanFlowPermissions.All)
        {
            if (!await db.PermissionDefinitions.AnyAsync(x => x.Key == permission))
            {
                db.PermissionDefinitions.Add(new PermissionDefinition
                {
                    Key = permission,
                    DisplayName = permission,
                    Module = permission.Split('.')[0]
                });
            }
        }

        await db.SaveChangesAsync();

        var adminRole = await db.HumanFlowRoles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Name == "Administrador RRHH");
        if (adminRole is null)
        {
            adminRole = new Role { TenantId = tenant.Id, Name = "Administrador RRHH", IsSystemRole = true };
            db.HumanFlowRoles.Add(adminRole);
            await db.SaveChangesAsync();

            var permissions = await db.PermissionDefinitions.ToListAsync();
            foreach (var permission in permissions)
            {
                db.RolePermissions.Add(new RolePermission
                {
                    TenantId = tenant.Id,
                    RoleId = adminRole.Id,
                    PermissionDefinitionId = permission.Id
                });
            }
            await db.SaveChangesAsync();
        }

        var admin = await userManager.FindByEmailAsync("admin@humanflow.local");
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin@humanflow.local",
                Email = "admin@humanflow.local",
                EmailConfirmed = true,
                DisplayName = "Platform Admin",
                PlatformRole = PlatformRole.PlatformAdministrator
            };

            var result = await userManager.CreateAsync(admin, "HumanFlow123");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            }
        }

        if (!await db.UserTenantMemberships.IgnoreQueryFilters().AnyAsync(x => x.UserId == admin.Id && x.TenantId == tenant.Id))
        {
            db.UserTenantMemberships.Add(new UserTenantMembership
            {
                TenantId = tenant.Id,
                UserId = admin.Id,
                RoleId = adminRole.Id
            });
        }

        if (!await db.OrganizationUnits.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant.Id && x.Name == "Recursos Humanos"))
        {
            db.OrganizationUnits.Add(new OrganizationUnit { TenantId = tenant.Id, Name = "Recursos Humanos", Code = "HR" });
            db.JobPositions.Add(new JobPosition { TenantId = tenant.Id, Title = "Analista de RRHH", Code = "HR-AN" });
            db.Contacts.Add(new Contact
            {
                TenantId = tenant.Id,
                Kind = ContactKind.External,
                Type = ContactType.Provider,
                DisplayName = "Estudio Legal Demo",
                Email = "legal@example.com"
            });
            db.Employees.Add(new Employee
            {
                TenantId = tenant.Id,
                EmployeeNumber = "EMP-001",
                FirstName = "Maria",
                LastName = "Gomez",
                Email = "maria.gomez@example.com",
                Status = EmployeeStatus.Active,
                HireDate = DateOnly.FromDateTime(DateTime.UtcNow.Date)
            });
        }

        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 2: Invoke seeder in Program.cs**

Add after `var app = builder.Build();`:

```csharp
await DbSeeder.SeedAsync(app.Services);
```

Add using:

```csharp
using HumanFlow.Infrastructure.Data;
```

- [ ] **Step 3: Create first migration**

Run:

```bash
dotnet ef migrations add InitialHumanFlowSchema --project src/HumanFlow.Infrastructure --startup-project src/HumanFlow.Web --context HumanFlowDbContext
```

Expected: migration files created under `src/HumanFlow.Infrastructure/Migrations`.

- [ ] **Step 4: Build**

Run:

```bash
dotnet build HumanFlow.slnx
```

Expected: build succeeds.

- [ ] **Step 5: Commit**

Run:

```bash
git add src/HumanFlow.Infrastructure src/HumanFlow.Web
git commit -m "feat: seed foundation data"
```

Expected: commit created.

---

## Task 9: Add Permission Checker

**Files:**
- Create: `src/HumanFlow.Infrastructure/Security/PermissionChecker.cs`
- Modify: `src/HumanFlow.Web/Program.cs`
- Create: `tests/HumanFlow.Tests/Security/PermissionCheckerTests.cs`

- [ ] **Step 1: Add permission checker test**

Create `tests/HumanFlow.Tests/Security/PermissionCheckerTests.cs`:

```csharp
using System.Security.Claims;
using HumanFlow.Application.Security;
using HumanFlow.Application.Tenancy;
using HumanFlow.Domain.Security;
using HumanFlow.Infrastructure.Data;
using HumanFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HumanFlow.Tests.Security;

public sealed class PermissionCheckerTests
{
    [Fact]
    public async Task user_has_permission_from_tenant_membership_role()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var currentTenant = new CurrentTenant();
        currentTenant.Set(tenantId, "demo");

        var options = new DbContextOptionsBuilder<HumanFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new HumanFlowDbContext(options, currentTenant);
        var permission = new PermissionDefinition
        {
            Key = HumanFlowPermissions.EmployeesRead,
            DisplayName = "Read employees",
            Module = "employees"
        };
        var role = new Role { TenantId = tenantId, Name = "Reader" };
        db.PermissionDefinitions.Add(permission);
        db.HumanFlowRoles.Add(role);
        await db.SaveChangesAsync();

        db.RolePermissions.Add(new RolePermission
        {
            TenantId = tenantId,
            RoleId = role.Id,
            PermissionDefinitionId = permission.Id
        });
        db.UserTenantMemberships.Add(new UserTenantMembership
        {
            TenantId = tenantId,
            UserId = userId,
            RoleId = role.Id
        });
        await db.SaveChangesAsync();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            "Test"));

        var checker = new PermissionChecker(db, currentTenant);
        var allowed = await checker.HasPermissionAsync(principal, HumanFlowPermissions.EmployeesRead, CancellationToken.None);

        Assert.True(allowed);
    }
}
```

- [ ] **Step 2: Run test and verify failure**

Run:

```bash
dotnet test tests/HumanFlow.Tests/HumanFlow.Tests.csproj --filter user_has_permission_from_tenant_membership_role
```

Expected: FAIL because `PermissionChecker` does not exist.

- [ ] **Step 3: Implement permission checker**

Create `src/HumanFlow.Infrastructure/Security/PermissionChecker.cs`:

```csharp
using System.Security.Claims;
using HumanFlow.Application.Security;
using HumanFlow.Application.Tenancy;
using HumanFlow.Domain.Security;
using HumanFlow.Infrastructure.Data;
using HumanFlow.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HumanFlow.Infrastructure.Security;

public sealed class PermissionChecker : IPermissionChecker
{
    private readonly HumanFlowDbContext _dbContext;
    private readonly ICurrentTenant _currentTenant;

    public PermissionChecker(HumanFlowDbContext dbContext, ICurrentTenant currentTenant)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
    }

    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission, CancellationToken cancellationToken)
    {
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return false;
        }

        var appUser = await _dbContext.Set<ApplicationUser>().FindAsync([userId], cancellationToken);
        if (appUser?.PlatformRole == PlatformRole.PlatformAdministrator)
        {
            return true;
        }

        if (!_currentTenant.TenantId.HasValue)
        {
            return false;
        }

        return await _dbContext.UserTenantMemberships
            .Where(x => x.UserId == userId && x.IsActive)
            .Join(_dbContext.RolePermissions,
                membership => membership.RoleId,
                rolePermission => rolePermission.RoleId,
                (membership, rolePermission) => rolePermission)
            .Join(_dbContext.PermissionDefinitions,
                rolePermission => rolePermission.PermissionDefinitionId,
                permissionDefinition => permissionDefinition.Id,
                (rolePermission, permissionDefinition) => permissionDefinition)
            .AnyAsync(x => x.Key == permission, cancellationToken);
    }
}
```

- [ ] **Step 4: Register permission checker**

Add to `src/HumanFlow.Web/Program.cs`:

```csharp
using HumanFlow.Application.Security;
using HumanFlow.Infrastructure.Security;
```

Add service registration:

```csharp
builder.Services.AddScoped<IPermissionChecker, PermissionChecker>();
```

- [ ] **Step 5: Run tests**

Run:

```bash
dotnet test HumanFlow.slnx
```

Expected: all tests pass.

- [ ] **Step 6: Commit**

Run:

```bash
git add src/HumanFlow.Infrastructure src/HumanFlow.Web tests/HumanFlow.Tests
git commit -m "feat: add tenant permission checker"
```

Expected: commit created.

---

## Task 10: Add Employee Application Service

**Files:**
- Create: `src/HumanFlow.Application/Employees/EmployeeListItem.cs`
- Create: `src/HumanFlow.Application/Employees/IEmployeeService.cs`
- Create: `src/HumanFlow.Infrastructure/Employees/EmployeeService.cs`
- Create: `tests/HumanFlow.Tests/Employees/EmployeeServiceTests.cs`
- Modify: `src/HumanFlow.Web/Program.cs`

- [ ] **Step 1: Add service test**

Create `tests/HumanFlow.Tests/Employees/EmployeeServiceTests.cs`:

```csharp
using HumanFlow.Application.Employees;
using HumanFlow.Application.Tenancy;
using HumanFlow.Domain.Employees;
using HumanFlow.Infrastructure.Data;
using HumanFlow.Infrastructure.Employees;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HumanFlow.Tests.Employees;

public sealed class EmployeeServiceTests
{
    [Fact]
    public async Task list_async_returns_employee_projection_for_current_tenant()
    {
        var tenantId = Guid.NewGuid();
        var currentTenant = new CurrentTenant();
        currentTenant.Set(tenantId, "demo");

        var options = new DbContextOptionsBuilder<HumanFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new HumanFlowDbContext(options, currentTenant);
        db.Employees.Add(new Employee
        {
            TenantId = tenantId,
            EmployeeNumber = "EMP-001",
            FirstName = "Lucia",
            LastName = "Perez",
            Status = EmployeeStatus.Active
        });
        await db.SaveChangesAsync();

        var service = new EmployeeService(db, currentTenant);
        var employees = await service.ListAsync(CancellationToken.None);

        Assert.Single(employees);
        Assert.Equal("Lucia Perez", employees[0].FullName);
        Assert.Equal("EMP-001", employees[0].EmployeeNumber);
    }
}
```

- [ ] **Step 2: Run test and verify failure**

Run:

```bash
dotnet test tests/HumanFlow.Tests/HumanFlow.Tests.csproj --filter list_async_returns_employee_projection_for_current_tenant
```

Expected: FAIL because employee service files do not exist.

- [ ] **Step 3: Implement employee service**

Create `src/HumanFlow.Application/Employees/EmployeeListItem.cs`:

```csharp
using HumanFlow.Domain.Employees;

namespace HumanFlow.Application.Employees;

public sealed record EmployeeListItem(
    Guid Id,
    string EmployeeNumber,
    string FullName,
    string? Email,
    EmployeeStatus Status);
```

Create `src/HumanFlow.Application/Employees/IEmployeeService.cs`:

```csharp
namespace HumanFlow.Application.Employees;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeListItem>> ListAsync(CancellationToken cancellationToken);
}
```

Create `src/HumanFlow.Infrastructure/Employees/EmployeeService.cs`:

```csharp
using HumanFlow.Application.Employees;
using HumanFlow.Application.Tenancy;
using HumanFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HumanFlow.Infrastructure.Employees;

public sealed class EmployeeService : IEmployeeService
{
    private readonly HumanFlowDbContext _dbContext;
    private readonly ICurrentTenant _currentTenant;

    public EmployeeService(HumanFlowDbContext dbContext, ICurrentTenant currentTenant)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<EmployeeListItem>> ListAsync(CancellationToken cancellationToken)
    {
        if (!_currentTenant.IsAvailable)
        {
            return [];
        }

        return await _dbContext.Employees
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new EmployeeListItem(
                x.Id,
                x.EmployeeNumber,
                x.FirstName + " " + x.LastName,
                x.Email,
                x.Status))
            .ToListAsync(cancellationToken);
    }
}
```

- [ ] **Step 4: Register service**

Add to `src/HumanFlow.Web/Program.cs`:

```csharp
using HumanFlow.Application.Employees;
using HumanFlow.Infrastructure.Employees;
```

Add service registration:

```csharp
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
```

- [ ] **Step 5: Run tests and build**

Run:

```bash
dotnet build HumanFlow.slnx
dotnet test HumanFlow.slnx
```

Expected: tests and build pass.

- [ ] **Step 6: Commit**

Run:

```bash
git add src/HumanFlow.Application src/HumanFlow.Infrastructure src/HumanFlow.Web tests/HumanFlow.Tests
git commit -m "feat: add employee listing service"
```

Expected: commit created.

---

## Task 11: Build First Blazor Shell And Employees Page

**Files:**
- Modify: `src/HumanFlow.Web/Components/Layout/NavMenu.razor`
- Create: `src/HumanFlow.Web/Components/Pages/Employees/Employees.razor`

- [ ] **Step 1: Add navigation links**

Modify `src/HumanFlow.Web/Components/Layout/NavMenu.razor` to include:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="employees">
        <span class="bi bi-people-fill-nav-menu" aria-hidden="true"></span> Empleados
    </NavLink>
</div>
<div class="nav-item px-3">
    <NavLink class="nav-link" href="contacts">
        <span class="bi bi-person-lines-fill-nav-menu" aria-hidden="true"></span> Contactos
    </NavLink>
</div>
<div class="nav-item px-3">
    <NavLink class="nav-link" href="admin/tenants">
        <span class="bi bi-building-fill-nav-menu" aria-hidden="true"></span> Tenants
    </NavLink>
</div>
```

- [ ] **Step 2: Add employees page**

Create `src/HumanFlow.Web/Components/Pages/Employees/Employees.razor`:

```razor
@page "/employees"
@rendermode InteractiveServer
@using HumanFlow.Application.Employees
@inject IEmployeeService EmployeeService

<PageTitle>Empleados</PageTitle>

<h1>Empleados</h1>

@if (_employees is null)
{
    <p>Cargando...</p>
}
else if (_employees.Count == 0)
{
    <p>No hay empleados para el tenant actual.</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Legajo</th>
                <th>Nombre</th>
                <th>Email</th>
                <th>Estado</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var employee in _employees)
            {
                <tr>
                    <td>@employee.EmployeeNumber</td>
                    <td>@employee.FullName</td>
                    <td>@employee.Email</td>
                    <td>@employee.Status</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private IReadOnlyList<EmployeeListItem>? _employees;

    protected override async Task OnInitializedAsync()
    {
        _employees = await EmployeeService.ListAsync(CancellationToken.None);
    }
}
```

- [ ] **Step 3: Build**

Run:

```bash
dotnet build HumanFlow.slnx
```

Expected: build succeeds.

- [ ] **Step 4: Run app**

Run:

```bash
dotnet run --project src/HumanFlow.Web
```

Expected: app starts. Open `/employees?tenant=demo` or send header `X-Tenant-Slug: demo`; page shows seeded employee.

- [ ] **Step 5: Commit**

Run:

```bash
git add src/HumanFlow.Web
git commit -m "feat: add Blazor employees page"
```

Expected: commit created.

---

## Task 12: Add Contact Service And Page

**Files:**
- Create: `src/HumanFlow.Application/Contacts/ContactListItem.cs`
- Create: `src/HumanFlow.Application/Contacts/IContactService.cs`
- Create: `src/HumanFlow.Infrastructure/Contacts/ContactService.cs`
- Create: `src/HumanFlow.Web/Components/Pages/Contacts/Contacts.razor`
- Modify: `src/HumanFlow.Web/Program.cs`

- [ ] **Step 1: Add contact service files**

Create `src/HumanFlow.Application/Contacts/ContactListItem.cs`:

```csharp
using HumanFlow.Domain.Contacts;

namespace HumanFlow.Application.Contacts;

public sealed record ContactListItem(
    Guid Id,
    string DisplayName,
    ContactKind Kind,
    ContactType Type,
    string? Email,
    string? Phone);
```

Create `src/HumanFlow.Application/Contacts/IContactService.cs`:

```csharp
namespace HumanFlow.Application.Contacts;

public interface IContactService
{
    Task<IReadOnlyList<ContactListItem>> ListAsync(CancellationToken cancellationToken);
}
```

Create `src/HumanFlow.Infrastructure/Contacts/ContactService.cs`:

```csharp
using HumanFlow.Application.Contacts;
using HumanFlow.Application.Tenancy;
using HumanFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HumanFlow.Infrastructure.Contacts;

public sealed class ContactService : IContactService
{
    private readonly HumanFlowDbContext _dbContext;
    private readonly ICurrentTenant _currentTenant;

    public ContactService(HumanFlowDbContext dbContext, ICurrentTenant currentTenant)
    {
        _dbContext = dbContext;
        _currentTenant = currentTenant;
    }

    public async Task<IReadOnlyList<ContactListItem>> ListAsync(CancellationToken cancellationToken)
    {
        if (!_currentTenant.IsAvailable)
        {
            return [];
        }

        return await _dbContext.Contacts
            .OrderBy(x => x.DisplayName)
            .Select(x => new ContactListItem(x.Id, x.DisplayName, x.Kind, x.Type, x.Email, x.Phone))
            .ToListAsync(cancellationToken);
    }
}
```

- [ ] **Step 2: Register contact service**

Add to `src/HumanFlow.Web/Program.cs`:

```csharp
using HumanFlow.Application.Contacts;
using HumanFlow.Infrastructure.Contacts;
```

Add service registration:

```csharp
builder.Services.AddScoped<IContactService, ContactService>();
```

- [ ] **Step 3: Add contacts page**

Create `src/HumanFlow.Web/Components/Pages/Contacts/Contacts.razor`:

```razor
@page "/contacts"
@rendermode InteractiveServer
@using HumanFlow.Application.Contacts
@inject IContactService ContactService

<PageTitle>Contactos</PageTitle>

<h1>Contactos</h1>

@if (_contacts is null)
{
    <p>Cargando...</p>
}
else if (_contacts.Count == 0)
{
    <p>No hay contactos para el tenant actual.</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Nombre</th>
                <th>Clase</th>
                <th>Tipo</th>
                <th>Email</th>
                <th>Telefono</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var contact in _contacts)
            {
                <tr>
                    <td>@contact.DisplayName</td>
                    <td>@contact.Kind</td>
                    <td>@contact.Type</td>
                    <td>@contact.Email</td>
                    <td>@contact.Phone</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private IReadOnlyList<ContactListItem>? _contacts;

    protected override async Task OnInitializedAsync()
    {
        _contacts = await ContactService.ListAsync(CancellationToken.None);
    }
}
```

- [ ] **Step 4: Build and commit**

Run:

```bash
dotnet build HumanFlow.slnx
git add src/HumanFlow.Application src/HumanFlow.Infrastructure src/HumanFlow.Web
git commit -m "feat: add contacts listing"
```

Expected: build succeeds and commit created.

---

## Task 13: Add Minimal Administration Pages

**Files:**
- Create: `src/HumanFlow.Web/Components/Pages/Admin/Tenants.razor`
- Create: `src/HumanFlow.Web/Components/Pages/Admin/Roles.razor`
- Create: `src/HumanFlow.Web/Components/Pages/Admin/Users.razor`

- [ ] **Step 1: Add tenants page**

Create `src/HumanFlow.Web/Components/Pages/Admin/Tenants.razor`:

```razor
@page "/admin/tenants"
@rendermode InteractiveServer
@using HumanFlow.Infrastructure.Data
@using Microsoft.EntityFrameworkCore
@inject HumanFlowDbContext DbContext

<PageTitle>Tenants</PageTitle>

<h1>Tenants</h1>

@if (_tenants is null)
{
    <p>Cargando...</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Nombre</th>
                <th>Slug</th>
                <th>Estado</th>
                <th>Almacenamiento</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var tenant in _tenants)
            {
                <tr>
                    <td>@tenant.Name</td>
                    <td>@tenant.Slug</td>
                    <td>@tenant.Status</td>
                    <td>@tenant.StorageMode</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private List<HumanFlow.Domain.Tenancy.Tenant>? _tenants;

    protected override async Task OnInitializedAsync()
    {
        _tenants = await DbContext.Tenants.IgnoreQueryFilters().OrderBy(x => x.Name).ToListAsync();
    }
}
```

- [ ] **Step 2: Add role and user administration summary pages with real data counts**

Create `src/HumanFlow.Web/Components/Pages/Admin/Roles.razor`:

```razor
@page "/admin/roles"
@rendermode InteractiveServer
@using HumanFlow.Infrastructure.Data
@using Microsoft.EntityFrameworkCore
@inject HumanFlowDbContext DbContext

<PageTitle>Roles</PageTitle>

<h1>Roles</h1>

<p>Total de roles del tenant actual: @_roleCount</p>

@code {
    private int _roleCount;

    protected override async Task OnInitializedAsync()
    {
        _roleCount = await DbContext.HumanFlowRoles.CountAsync();
    }
}
```

Create `src/HumanFlow.Web/Components/Pages/Admin/Users.razor`:

```razor
@page "/admin/users"
@rendermode InteractiveServer
@using HumanFlow.Infrastructure.Data
@using HumanFlow.Infrastructure.Identity
@using Microsoft.EntityFrameworkCore
@inject HumanFlowDbContext DbContext

<PageTitle>Usuarios</PageTitle>

<h1>Usuarios</h1>

@if (_users is null)
{
    <p>Cargando...</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Email</th>
                <th>Nombre</th>
                <th>Rol plataforma</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var user in _users)
            {
                <tr>
                    <td>@user.Email</td>
                    <td>@user.DisplayName</td>
                    <td>@user.PlatformRole</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private List<ApplicationUser>? _users;

    protected override async Task OnInitializedAsync()
    {
        _users = await DbContext.Users.OrderBy(x => x.Email).ToListAsync();
    }
}
```

- [ ] **Step 3: Build and commit**

Run:

```bash
dotnet build HumanFlow.slnx
git add src/HumanFlow.Web
git commit -m "feat: add minimal admin pages"
```

Expected: build succeeds and commit created.

---

## Task 14: Add Master Data Pages

**Files:**
- Create: `src/HumanFlow.Web/Components/Pages/MasterData/OrganizationUnits.razor`
- Create: `src/HumanFlow.Web/Components/Pages/MasterData/JobPositions.razor`

- [ ] **Step 1: Add organization units page**

Create `src/HumanFlow.Web/Components/Pages/MasterData/OrganizationUnits.razor`:

```razor
@page "/master-data/organization-units"
@rendermode InteractiveServer
@using HumanFlow.Infrastructure.Data
@using Microsoft.EntityFrameworkCore
@inject HumanFlowDbContext DbContext

<PageTitle>Unidades organizativas</PageTitle>

<h1>Unidades organizativas</h1>

@if (_items is null)
{
    <p>Cargando...</p>
}
else
{
    <ul>
        @foreach (var item in _items)
        {
            <li>@item.Code - @item.Name</li>
        }
    </ul>
}

@code {
    private List<HumanFlow.Domain.Organization.OrganizationUnit>? _items;

    protected override async Task OnInitializedAsync()
    {
        _items = await DbContext.OrganizationUnits.OrderBy(x => x.Name).ToListAsync();
    }
}
```

- [ ] **Step 2: Add job positions page**

Create `src/HumanFlow.Web/Components/Pages/MasterData/JobPositions.razor`:

```razor
@page "/master-data/job-positions"
@rendermode InteractiveServer
@using HumanFlow.Infrastructure.Data
@using Microsoft.EntityFrameworkCore
@inject HumanFlowDbContext DbContext

<PageTitle>Puestos</PageTitle>

<h1>Puestos</h1>

@if (_items is null)
{
    <p>Cargando...</p>
}
else
{
    <ul>
        @foreach (var item in _items)
        {
            <li>@item.Code - @item.Title</li>
        }
    </ul>
}

@code {
    private List<HumanFlow.Domain.Organization.JobPosition>? _items;

    protected override async Task OnInitializedAsync()
    {
        _items = await DbContext.JobPositions.OrderBy(x => x.Title).ToListAsync();
    }
}
```

- [ ] **Step 3: Build and commit**

Run:

```bash
dotnet build HumanFlow.slnx
git add src/HumanFlow.Web
git commit -m "feat: add master data pages"
```

Expected: build succeeds and commit created.

---

## Task 15: Final Verification

**Files:**
- Modify only if verification reveals compile or test issues.

- [ ] **Step 1: Run full build**

Run:

```bash
dotnet build HumanFlow.slnx
```

Expected: build succeeds with 0 errors.

- [ ] **Step 2: Run full tests**

Run:

```bash
dotnet test HumanFlow.slnx
```

Expected: all tests pass.

- [ ] **Step 3: Run app**

Run:

```bash
dotnet run --project src/HumanFlow.Web
```

Expected: app starts. Validate these URLs:

```text
/employees?tenant=demo
/contacts?tenant=demo
/admin/tenants
/admin/users
/admin/roles?tenant=demo
/master-data/organization-units?tenant=demo
/master-data/job-positions?tenant=demo
```

- [ ] **Step 4: Confirm repository state and commit verification fixes**

Run:

```bash
git status --short
```

Expected: no output when verification made no changes.

If the command lists files that were changed only to fix build or test failures, run:

```bash
git add .
git commit -m "fix: stabilize MVP foundation"
```

Expected: no uncommitted implementation changes remain.

---

## Self-Review

Spec coverage:

- Multi-tenant hybrid foundation: covered by tenant catalog, storage mode, resolver, middleware, tenant filters, and isolation tests.
- SQL Server to Azure SQL portability: covered by SQL Server EF provider, no SQL Agent or linked server dependency, and no local file storage in schema.
- Users, global admins, tenant memberships: covered by `ApplicationUser`, `PlatformRole`, and `UserTenantMembership`.
- Roles and granular permissions: covered by role, permission definition, role permission, seeding, and permission checker.
- Employee file foundation: covered by employee, document metadata, event timeline, and employees UI.
- Contacts: covered by contact domain, service, seed, and UI.
- Master data: covered by organization units and job positions.
- Testing: covered by tenant isolation, tenant resolution, permission checker, and employee service tests.

Known intentional limits for this plan:

- CRUD forms are minimal in this first implementation plan; list/read flows and seed data establish the foundation before richer editing workflows.
- Dedicated database routing is modeled but not yet activated; the MVP starts in shared-database mode and keeps the contract ready for expansion.
- SSO providers are not implemented; the schema and identity boundary stay ready for a later authentication-provider module.
