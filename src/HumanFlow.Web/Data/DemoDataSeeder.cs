using HumanFlow.Domain.Contacts;
using HumanFlow.Domain.Employees;
using HumanFlow.Domain.Organization;
using HumanFlow.Domain.Security;
using HumanFlow.Domain.Tenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HumanFlow.Web.Data;

public static class DemoDataSeeder
{
    private static readonly string[] PermissionKeys =
    [
        "platform.tenants.manage",
        "users.manage",
        "roles.manage",
        "tenant.settings.manage",
        "employees.read",
        "employees.create",
        "employees.update",
        "employees.documents.manage",
        "contacts.read",
        "contacts.manage",
        "masterdata.manage"
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.EnsureCreatedAsync();

        var demoTenant = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == "demo");
        if (demoTenant is null)
        {
            demoTenant = new Tenant
            {
                Name = "Demo Company",
                Slug = "demo",
                PrimaryDomain = "demo.humanflow.local",
                StorageMode = TenantStorageMode.SharedDatabase,
                Status = TenantStatus.Active
            };
            db.Tenants.Add(demoTenant);
            await db.SaveChangesAsync();
        }

        var regulatedTenant = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == "regulated");
        if (regulatedTenant is null)
        {
            db.Tenants.Add(new Tenant
            {
                Name = "Regulated Health Group",
                Slug = "regulated",
                PrimaryDomain = "rrhh.health.example",
                StorageMode = TenantStorageMode.DedicatedDatabase,
                Status = TenantStatus.Provisioning
            });
        }

        foreach (var permissionKey in PermissionKeys)
        {
            if (!await db.PermissionDefinitions.AnyAsync(x => x.Key == permissionKey))
            {
                db.PermissionDefinitions.Add(new PermissionDefinition
                {
                    Key = permissionKey,
                    DisplayName = permissionKey,
                    Module = permissionKey.Split('.')[0]
                });
            }
        }

        if (!await db.HumanFlowRoles.AnyAsync(x => x.TenantId == demoTenant.Id))
        {
            db.HumanFlowRoles.AddRange(
                new Role
                {
                    TenantId = demoTenant.Id,
                    Name = "Administrador RRHH",
                    Description = "Gestiona usuarios, roles, legajos, contactos y datos maestros del tenant.",
                    IsSystemRole = true
                },
                new Role
                {
                    TenantId = demoTenant.Id,
                    Name = "Analista RRHH",
                    Description = "Opera el legajo y mantiene datos de empleados y contactos.",
                    IsSystemRole = true
                },
                new Role
                {
                    TenantId = demoTenant.Id,
                    Name = "Auditor",
                    Description = "Consulta informacion sensible sin editarla.",
                    IsSystemRole = true
                });
        }

        if (!await db.OrganizationUnits.AnyAsync(x => x.TenantId == demoTenant.Id))
        {
            db.OrganizationUnits.AddRange(
                new OrganizationUnit { TenantId = demoTenant.Id, Name = "Recursos Humanos", Code = "HR" },
                new OrganizationUnit { TenantId = demoTenant.Id, Name = "Tecnologia", Code = "IT" },
                new OrganizationUnit { TenantId = demoTenant.Id, Name = "Operaciones", Code = "OPS" });
        }

        if (!await db.JobPositions.AnyAsync(x => x.TenantId == demoTenant.Id))
        {
            db.JobPositions.AddRange(
                new JobPosition { TenantId = demoTenant.Id, Title = "Analista de RRHH", Code = "HR-AN" },
                new JobPosition { TenantId = demoTenant.Id, Title = "Lider Tecnico", Code = "IT-LT" },
                new JobPosition { TenantId = demoTenant.Id, Title = "Coordinador de Operaciones", Code = "OPS-CO" });
        }

        if (!await db.Contacts.AnyAsync(x => x.TenantId == demoTenant.Id))
        {
            db.Contacts.AddRange(
                new Contact
                {
                    TenantId = demoTenant.Id,
                    Kind = ContactKind.External,
                    Type = ContactType.Legal,
                    DisplayName = "Estudio Legal Demo",
                    OrganizationName = "Estudio Legal Demo",
                    Email = "legal@example.com",
                    Phone = "+54 11 4000-1000"
                },
                new Contact
                {
                    TenantId = demoTenant.Id,
                    Kind = ContactKind.External,
                    Type = ContactType.Medical,
                    DisplayName = "Prestador Medico Norte",
                    OrganizationName = "PMN Salud",
                    Email = "rrhh@pmn.example",
                    Phone = "+54 11 4000-2000"
                },
                new Contact
                {
                    TenantId = demoTenant.Id,
                    Kind = ContactKind.Internal,
                    Type = ContactType.Operational,
                    DisplayName = "Mesa de Ayuda Interna",
                    Email = "soporte@demo.local"
                });
        }

        if (!await db.Employees.AnyAsync(x => x.TenantId == demoTenant.Id))
        {
            db.Employees.AddRange(
                new Employee
                {
                    TenantId = demoTenant.Id,
                    EmployeeNumber = "EMP-001",
                    FirstName = "Maria",
                    LastName = "Gomez",
                    Email = "maria.gomez@demo.local",
                    Phone = "+54 11 5000-0001",
                    Status = EmployeeStatus.Active,
                    HireDate = new DateOnly(2022, 3, 14)
                },
                new Employee
                {
                    TenantId = demoTenant.Id,
                    EmployeeNumber = "EMP-002",
                    FirstName = "Lucia",
                    LastName = "Perez",
                    Email = "lucia.perez@demo.local",
                    Phone = "+54 11 5000-0002",
                    Status = EmployeeStatus.OnLeave,
                    HireDate = new DateOnly(2021, 9, 1)
                },
                new Employee
                {
                    TenantId = demoTenant.Id,
                    EmployeeNumber = "EMP-003",
                    FirstName = "Tomas",
                    LastName = "Rivas",
                    Email = "tomas.rivas@demo.local",
                    Phone = "+54 11 5000-0003",
                    Status = EmployeeStatus.Active,
                    HireDate = new DateOnly(2023, 1, 23)
                });
        }

        var admin = await userManager.FindByEmailAsync("admin@humanflow.local");
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin@humanflow.local",
                Email = "admin@humanflow.local",
                EmailConfirmed = true,
                DisplayName = "Administrador Plataforma",
                PlatformRole = PlatformRole.PlatformAdministrator
            };

            var result = await userManager.CreateAsync(admin, "HumanFlow123!");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            }
        }

        await db.SaveChangesAsync();
    }
}
