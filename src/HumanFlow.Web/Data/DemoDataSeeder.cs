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

        await db.Database.MigrateAsync();

        // ── Tenants ──────────────────────────────────────────────
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

        if (!await db.Tenants.AnyAsync(x => x.Slug == "regulated"))
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

        // ── Permisos ──────────────────────────────────────────────
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

        // ── Roles ─────────────────────────────────────────────────
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

        // ── Áreas y Puestos ───────────────────────────────────────
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

        // ── Contactos institucionales ─────────────────────────────
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

        // ── Empleados ─────────────────────────────────────────────
        if (!await db.Employees.AnyAsync(x => x.TenantId == demoTenant.Id))
        {
            db.Employees.AddRange(
                new Employee
                {
                    TenantId = demoTenant.Id,
                    EmployeeNumber = "EMP-001",
                    FirstName = "María",
                    LastName = "Gómez",
                    Email = "maria.gomez@demo.local",
                    PersonalEmail = "maria.gomez@gmail.com",
                    Phone = "+54 11 5000-0001",
                    NationalId = "28.456.789",
                    TaxId = "27-28456789-4",
                    BirthDate = new DateOnly(1988, 6, 12),
                    MaritalStatus = MaritalStatus.Married,
                    Nationality = "Argentina",
                    AddressStreet = "Av. Corrientes 1234 Piso 3",
                    AddressCity = "Buenos Aires",
                    AddressState = "CABA",
                    AddressPostalCode = "C1043",
                    AddressCountry = "Argentina",
                    Status = EmployeeStatus.Active,
                    HireDate = new DateOnly(2022, 3, 14)
                },
                new Employee
                {
                    TenantId = demoTenant.Id,
                    EmployeeNumber = "EMP-002",
                    FirstName = "Lucía",
                    LastName = "Pérez",
                    Email = "lucia.perez@demo.local",
                    Phone = "+54 11 5000-0002",
                    NationalId = "32.100.200",
                    TaxId = "23-32100200-9",
                    BirthDate = new DateOnly(1993, 11, 3),
                    MaritalStatus = MaritalStatus.Single,
                    Nationality = "Argentina",
                    AddressCity = "Rosario",
                    AddressState = "Santa Fe",
                    AddressCountry = "Argentina",
                    Status = EmployeeStatus.OnLeave,
                    HireDate = new DateOnly(2021, 9, 1)
                },
                new Employee
                {
                    TenantId = demoTenant.Id,
                    EmployeeNumber = "EMP-003",
                    FirstName = "Tomás",
                    LastName = "Rivas",
                    Email = "tomas.rivas@demo.local",
                    PersonalEmail = "trivas@hotmail.com",
                    Phone = "+54 11 5000-0003",
                    NationalId = "35.678.901",
                    TaxId = "20-35678901-3",
                    BirthDate = new DateOnly(1996, 4, 25),
                    MaritalStatus = MaritalStatus.Single,
                    Nationality = "Argentina",
                    AddressStreet = "San Martín 456",
                    AddressCity = "Córdoba",
                    AddressState = "Córdoba",
                    AddressPostalCode = "X5000",
                    AddressCountry = "Argentina",
                    Status = EmployeeStatus.Active,
                    HireDate = new DateOnly(2023, 1, 23)
                });

            await db.SaveChangesAsync();
        }

        // ── Historial de puestos y contactos de emergencia ────────
        var emp1 = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == "EMP-001" && e.TenantId == demoTenant.Id);
        var emp2 = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == "EMP-002" && e.TenantId == demoTenant.Id);
        var emp3 = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == "EMP-003" && e.TenantId == demoTenant.Id);

        var posHR = await db.JobPositions.FirstOrDefaultAsync(j => j.TenantId == demoTenant.Id && j.Code == "HR-AN");
        var posIT = await db.JobPositions.FirstOrDefaultAsync(j => j.TenantId == demoTenant.Id && j.Code == "IT-LT");
        var posOPS = await db.JobPositions.FirstOrDefaultAsync(j => j.TenantId == demoTenant.Id && j.Code == "OPS-CO");
        var unitHR = await db.OrganizationUnits.FirstOrDefaultAsync(u => u.TenantId == demoTenant.Id && u.Code == "HR");
        var unitIT = await db.OrganizationUnits.FirstOrDefaultAsync(u => u.TenantId == demoTenant.Id && u.Code == "IT");
        var unitOPS = await db.OrganizationUnits.FirstOrDefaultAsync(u => u.TenantId == demoTenant.Id && u.Code == "OPS");

        if (emp1 is not null && posHR is not null && posIT is not null && unitHR is not null && unitIT is not null
            && !await db.PositionHistories.AnyAsync(p => p.EmployeeId == emp1.Id))
        {
            // María: ingreso como Analista HR, luego promovida a Líder Técnico
            db.PositionHistories.AddRange(
                new PositionHistory
                {
                    TenantId = demoTenant.Id,
                    EmployeeId = emp1.Id,
                    JobPositionId = posHR.Id,
                    OrganizationUnitId = unitHR.Id,
                    StartDate = new DateOnly(2022, 3, 14),
                    EndDate = new DateOnly(2023, 1, 15),
                    Reason = PositionChangeReason.Hire,
                    Notes = "Ingreso como analista junior."
                },
                new PositionHistory
                {
                    TenantId = demoTenant.Id,
                    EmployeeId = emp1.Id,
                    JobPositionId = posIT.Id,
                    OrganizationUnitId = unitIT.Id,
                    StartDate = new DateOnly(2023, 1, 16),
                    EndDate = null,
                    Reason = PositionChangeReason.Promotion,
                    Notes = "Promoción por desempeño destacado."
                });

            // Actualizar puesto actual en Employee
            emp1.JobPositionId = posIT.Id;
            emp1.OrganizationUnitId = unitIT.Id;
        }

        if (emp2 is not null && posOPS is not null && unitOPS is not null
            && !await db.PositionHistories.AnyAsync(p => p.EmployeeId == emp2.Id))
        {
            db.PositionHistories.Add(new PositionHistory
            {
                TenantId = demoTenant.Id,
                EmployeeId = emp2.Id,
                JobPositionId = posOPS.Id,
                OrganizationUnitId = unitOPS.Id,
                StartDate = new DateOnly(2021, 9, 1),
                EndDate = null,
                Reason = PositionChangeReason.Hire
            });

            emp2.JobPositionId = posOPS.Id;
            emp2.OrganizationUnitId = unitOPS.Id;
        }

        if (emp3 is not null && posHR is not null && unitHR is not null && emp1 is not null
            && !await db.PositionHistories.AnyAsync(p => p.EmployeeId == emp3.Id))
        {
            db.PositionHistories.Add(new PositionHistory
            {
                TenantId = demoTenant.Id,
                EmployeeId = emp3.Id,
                JobPositionId = posHR.Id,
                OrganizationUnitId = unitHR.Id,
                ManagerEmployeeId = emp1.Id,
                StartDate = new DateOnly(2023, 1, 23),
                EndDate = null,
                Reason = PositionChangeReason.Hire
            });

            emp3.JobPositionId = posHR.Id;
            emp3.OrganizationUnitId = unitHR.Id;
            emp3.ManagerEmployeeId = emp1.Id;
        }

        // Contactos de emergencia
        if (emp1 is not null && !await db.EmployeeContacts.AnyAsync(c => c.EmployeeId == emp1.Id))
        {
            db.EmployeeContacts.AddRange(
                new EmployeeContact
                {
                    TenantId = demoTenant.Id,
                    EmployeeId = emp1.Id,
                    FullName = "Carlos Gómez",
                    Relationship = ContactRelationship.Father,
                    Phone = "+54 11 5000-9001",
                    IsEmergencyContact = true,
                    IsPrimaryEmergencyContact = true
                },
                new EmployeeContact
                {
                    TenantId = demoTenant.Id,
                    EmployeeId = emp1.Id,
                    FullName = "Roberto García",
                    Relationship = ContactRelationship.Spouse,
                    Phone = "+54 11 5000-9002",
                    Email = "roberto.garcia@gmail.com",
                    IsEmergencyContact = true,
                    IsPrimaryEmergencyContact = false
                });
        }

        if (emp3 is not null && !await db.EmployeeContacts.AnyAsync(c => c.EmployeeId == emp3.Id))
        {
            db.EmployeeContacts.Add(new EmployeeContact
            {
                TenantId = demoTenant.Id,
                EmployeeId = emp3.Id,
                FullName = "Ana Rivas",
                Relationship = ContactRelationship.Mother,
                Phone = "+54 11 5000-9003",
                IsEmergencyContact = true,
                IsPrimaryEmergencyContact = true
            });
        }

        // ── Evaluaciones de desempeño ─────────────────────────────
        if (emp1 is not null && !await db.PerformanceReviews.AnyAsync(r => r.EmployeeId == emp1.Id))
        {
            db.PerformanceReviews.AddRange(
                new PerformanceReview
                {
                    TenantId = demoTenant.Id,
                    EmployeeId = emp1.Id,
                    PeriodType = ReviewPeriodType.Annual,
                    PeriodYear = 2022,
                    PeriodNumber = 1,
                    ReviewDate = new DateOnly(2022, 12, 15),
                    OverallRating = ReviewRating.ExceedsExpectations,
                    StrengthsNotes = "Excelente capacidad de resolución de conflictos y liderazgo informal. Muy buen manejo de las herramientas del sector.",
                    ImprovementNotes = "Mejorar la documentación de procesos internos.",
                    GoalsNotes = "Certificación en HRIS durante el 1er semestre de 2023. Tomar curso de liderazgo.",
                    Status = ReviewStatus.Acknowledged,
                    AcknowledgedDate = new DateOnly(2022, 12, 20)
                },
                new PerformanceReview
                {
                    TenantId = demoTenant.Id,
                    EmployeeId = emp1.Id,
                    PeriodType = ReviewPeriodType.Annual,
                    PeriodYear = 2023,
                    PeriodNumber = 1,
                    ReviewDate = new DateOnly(2024, 1, 10),
                    OverallRating = ReviewRating.Outstanding,
                    StrengthsNotes = "Liderazgo técnico sobresaliente. Mentorea efectivamente al equipo junior. Resolvió el proyecto de migración HRIS en tiempo y forma.",
                    ImprovementNotes = "Desarrollar habilidades de presentación ejecutiva.",
                    GoalsNotes = "Liderar la implementación de la Fase B del HRIS. Presentar roadmap al Directorio en Q2 2024.",
                    Status = ReviewStatus.Acknowledged,
                    AcknowledgedDate = new DateOnly(2024, 1, 18)
                },
                new PerformanceReview
                {
                    TenantId = demoTenant.Id,
                    EmployeeId = emp1.Id,
                    PeriodType = ReviewPeriodType.SemiAnnual,
                    PeriodYear = 2024,
                    PeriodNumber = 1,
                    ReviewDate = new DateOnly(2024, 7, 15),
                    OverallRating = ReviewRating.Outstanding,
                    StrengthsNotes = "Implementación impecable del módulo de legajos. Alta proactividad y visión de producto.",
                    Status = ReviewStatus.Completed
                });
        }

        if (emp2 is not null && !await db.PerformanceReviews.AnyAsync(r => r.EmployeeId == emp2.Id))
        {
            db.PerformanceReviews.AddRange(
                new PerformanceReview
                {
                    TenantId = demoTenant.Id,
                    EmployeeId = emp2.Id,
                    PeriodType = ReviewPeriodType.Annual,
                    PeriodYear = 2022,
                    PeriodNumber = 1,
                    ReviewDate = new DateOnly(2023, 1, 5),
                    OverallRating = ReviewRating.MeetsExpectations,
                    StrengthsNotes = "Coordinación operativa sólida. Cumple objetivos dentro de los plazos establecidos.",
                    ImprovementNotes = "Fortalecer la comunicación interdepartamental.",
                    GoalsNotes = "Revisar y estandarizar los procedimientos de onboarding en Q1 2023.",
                    Status = ReviewStatus.Acknowledged,
                    AcknowledgedDate = new DateOnly(2023, 1, 12)
                },
                new PerformanceReview
                {
                    TenantId = demoTenant.Id,
                    EmployeeId = emp2.Id,
                    PeriodType = ReviewPeriodType.Annual,
                    PeriodYear = 2023,
                    PeriodNumber = 1,
                    ReviewDate = new DateOnly(2024, 1, 25),
                    OverallRating = ReviewRating.ExceedsExpectations,
                    StrengthsNotes = "Superó los objetivos de reducción de tiempos operativos en un 20%. Lideró la transición del equipo durante la licencia de la gerente.",
                    Status = ReviewStatus.Completed
                });
        }

        if (emp3 is not null && emp1 is not null && !await db.PerformanceReviews.AnyAsync(r => r.EmployeeId == emp3.Id))
        {
            db.PerformanceReviews.Add(new PerformanceReview
            {
                TenantId = demoTenant.Id,
                EmployeeId = emp3.Id,
                PeriodType = ReviewPeriodType.Annual,
                PeriodYear = 2023,
                PeriodNumber = 1,
                ReviewDate = new DateOnly(2024, 1, 20),
                ReviewerEmployeeId = emp1.Id,
                OverallRating = ReviewRating.MeetsExpectations,
                StrengthsNotes = "Primera evaluación anual. Muy buen proceso de adaptación. Asimila rápidamente los procedimientos.",
                ImprovementNotes = "Necesita mayor autonomía en la toma de decisiones. Continuar desarrollando habilidades de análisis.",
                GoalsNotes = "Completar la certificación interna de procesos RRHH en H1 2024.",
                Status = ReviewStatus.Acknowledged,
                AcknowledgedDate = new DateOnly(2024, 1, 26)
            });
        }

        // ── Usuario admin ─────────────────────────────────────────
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
