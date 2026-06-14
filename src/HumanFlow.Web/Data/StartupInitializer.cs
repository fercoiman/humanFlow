using HumanFlow.Domain.Contacts;
using HumanFlow.Domain.Employees;
using HumanFlow.Domain.Geography;
using HumanFlow.Domain.Organization;
using HumanFlow.Domain.Recruitment;
using HumanFlow.Domain.Security;
using HumanFlow.Domain.Tenancy;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace HumanFlow.Web.Data;

/// <summary>
/// Verifica el estado de la base de datos al arrancar y siembra datos demo
/// únicamente cuando la DB no existe (primera ejecución).
/// </summary>
public static class StartupInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger      = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        // ── 1. Verificar si la base de datos existe ───────────────────
        bool dbExists;
        try
        {
            dbExists = await db.Database.CanConnectAsync();
        }
        catch
        {
            dbExists = false;
        }

        // ── 2a. DB existe → solo aplicar migraciones pendientes ────────
        if (dbExists)
        {
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();

            if (pending.Count == 0)
            {
                logger.LogInformation(
                    "HumanFlow: base de datos verificada y actualizada. Iniciando sistema.");
            }
            else
            {
                logger.LogInformation(
                    "HumanFlow: aplicando {Count} migración(es) pendiente(s): {Names}",
                    pending.Count, string.Join(", ", pending));

                await db.Database.MigrateAsync();

                logger.LogInformation("HumanFlow: migraciones aplicadas correctamente.");
            }

            return;
        }

        // ── 2b. DB no existe → crear esquema y sembrar datos demo ──────
        logger.LogInformation(
            "HumanFlow: base de datos no encontrada. Creando esquema e inicializando datos de ejemplo...");

        await CreateSchemaAsync(db, logger);
        await SeedDemoDataAsync(db, userManager, logger);

        logger.LogInformation(
            "HumanFlow: sistema inicializado correctamente con datos de demostración.");
    }

    // ─── Crear esquema (con manejo de .mdf huérfanos en LocalDB) ────────
    private static async Task CreateSchemaAsync(ApplicationDbContext db, ILogger logger)
    {
        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 5170)
        {
            // LocalDB perdió el registro de la DB pero el .mdf sigue en disco.
            // Eliminamos los archivos huérfanos y recreamos.
            logger.LogWarning(
                "HumanFlow: archivos .mdf huérfanos detectados. Eliminando y recreando la base de datos...");

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            foreach (var fileName in new[] { "HumanFlow.mdf", "HumanFlow_log.ldf" })
            {
                var path = Path.Combine(userProfile, fileName);
                if (File.Exists(path)) File.Delete(path);
            }

            await db.Database.MigrateAsync();
        }
    }

    // ─── Banner de diagnóstico (se imprime cuando Kestrel ya escucha) ───
    public static async Task PrintBannerAsync(IServiceProvider services)
    {
        // ── Información de base de datos ──────────────────────────────
        string dbEngine       = "(no disponible)";
        string dbName         = "(no disponible)";
        string dbPhysicalFile = "(no disponible)";
        bool   dbConnected    = false;
        bool   identityOk     = false;

        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(100))
                        + ' — ' +
                    CAST(SERVERPROPERTY('Edition') AS NVARCHAR(100))  AS [Engine],
                    DB_NAME()                                           AS [Name],
                    (SELECT TOP 1 physical_name
                     FROM sys.master_files
                     WHERE database_id = DB_ID() AND type_desc = 'ROWS') AS [File]";

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                dbEngine       = reader.IsDBNull(0) ? dbEngine       : reader.GetString(0);
                dbName         = reader.IsDBNull(1) ? dbName         : reader.GetString(1);
                dbPhysicalFile = reader.IsDBNull(2) ? dbPhysicalFile : reader.GetString(2);
            }

            dbConnected = true;

            // Verificar Identity: si podemos contar usuarios, las tablas existen
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _ = await userManager.Users.CountAsync();
            identityOk = true;
        }
        catch
        {
            // Los valores por defecto ya indican el error
        }

        // ── URLs en las que escucha Kestrel ───────────────────────────
        var server        = services.GetService<IServer>();
        var addressFeature = server?.Features.Get<IServerAddressesFeature>();
        var listenUrls    = addressFeature?.Addresses?.OrderBy(u => u).ToList()
                          ?? new List<string>();

        string hostname;
        try   { hostname = Dns.GetHostName(); }
        catch { hostname = "(no disponible)"; }

        // ── Imprimir banner ───────────────────────────────────────────
        var line = new string('─', 70);
        var ok   = "[  OK  ]";
        var fail = "[ FAIL ]";

        Console.WriteLine();
        Console.WriteLine($"  {line}");
        Console.WriteLine("  HumanFlow HRMS  ·  Sistema de Gestión de Recursos Humanos");
        Console.WriteLine($"  {line}");
        Console.WriteLine();
        Console.WriteLine("  SERVICIOS");
        Console.WriteLine($"  {(dbConnected ? ok : fail)}  Base de datos SQL Server");
        Console.WriteLine($"  {(identityOk  ? ok : fail)}  Identity / Autenticación");
        Console.WriteLine($"  {ok}  Blazor Server (InteractiveServer)");
        Console.WriteLine();
        Console.WriteLine("  BASE DE DATOS");
        Console.WriteLine($"  Motor    :  {dbEngine}");
        Console.WriteLine($"  Nombre   :  {dbName}");
        Console.WriteLine($"  Archivo  :  {dbPhysicalFile}");
        Console.WriteLine();
        Console.WriteLine("  INTERFAZ WEB");
        Console.WriteLine($"  Host     :  {hostname}");

        if (listenUrls.Count == 0)
        {
            Console.WriteLine("  Puerto   :  (consultá los logs de Kestrel arriba)");
        }
        else
        {
            foreach (var url in listenUrls)
                Console.WriteLine($"  Escucha  :  {url}");
        }

        Console.WriteLine();
        Console.WriteLine($"  {line}");
        Console.WriteLine();
    }

    // ─── Datos demo mínimos (solo en primera ejecución) ─────────────────
    private static async Task SeedDemoDataAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        // ── Tenant ────────────────────────────────────────────────────
        logger.LogInformation("HumanFlow: creando tenant de demostración...");

        var tenant = new Tenant
        {
            Name          = "Demo Company",
            Slug          = "demo",
            PrimaryDomain = "demo.humanflow.local",
            StorageMode   = TenantStorageMode.SharedDatabase,
            Status        = TenantStatus.Active
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        // ── Permisos ──────────────────────────────────────────────────
        string[] permKeys =
        [
            "platform.tenants.manage", "users.manage", "roles.manage",
            "tenant.settings.manage",  "employees.read", "employees.create",
            "employees.update",        "employees.documents.manage",
            "contacts.read",           "contacts.manage", "masterdata.manage"
        ];

        foreach (var key in permKeys)
        {
            db.PermissionDefinitions.Add(new PermissionDefinition
            {
                Key         = key,
                DisplayName = key,
                Module      = key.Split('.')[0]
            });
        }

        // ── Roles ─────────────────────────────────────────────────────
        db.HumanFlowRoles.AddRange(
            new Role { TenantId = tenant.Id, Name = "Administrador RRHH", Description = "Acceso total al módulo de RRHH.", IsSystemRole = true },
            new Role { TenantId = tenant.Id, Name = "Analista RRHH",      Description = "Gestión de legajos y contactos.",  IsSystemRole = true },
            new Role { TenantId = tenant.Id, Name = "Auditor",             Description = "Solo lectura.",                    IsSystemRole = true }
        );

        // ── Geografía ─────────────────────────────────────────────────
        logger.LogInformation("HumanFlow: creando datos geográficos de demostración...");

        var gArgentina = new Country { TenantId = tenant.Id, Name = "Argentina", IsoCode = "AR", IsActive = true };
        var gChile     = new Country { TenantId = tenant.Id, Name = "Chile",     IsoCode = "CL", IsActive = true };
        var gUruguay   = new Country { TenantId = tenant.Id, Name = "Uruguay",   IsoCode = "UY", IsActive = true };
        db.Countries.AddRange(gArgentina, gChile, gUruguay);
        await db.SaveChangesAsync();

        var cBsAs    = new City { TenantId = tenant.Id, CountryId = gArgentina.Id, Name = "Buenos Aires", StateName = "Ciudad Autónoma de Buenos Aires", IsActive = true };
        var cCordoba = new City { TenantId = tenant.Id, CountryId = gArgentina.Id, Name = "Córdoba",      StateName = "Córdoba",                        IsActive = true };
        var cRosario = new City { TenantId = tenant.Id, CountryId = gArgentina.Id, Name = "Rosario",      StateName = "Santa Fe",                       IsActive = true };
        db.Cities.AddRange(cBsAs, cCordoba, cRosario);
        await db.SaveChangesAsync();

        var lPalermo     = new Locality { TenantId = tenant.Id, CityId = cBsAs.Id,    Name = "Palermo",       PostalCode = "C1425", IsActive = true };
        var lNvaCordoba  = new Locality { TenantId = tenant.Id, CityId = cCordoba.Id, Name = "Nueva Córdoba", PostalCode = "X5000", IsActive = true };
        var lPichincha   = new Locality { TenantId = tenant.Id, CityId = cRosario.Id, Name = "Pichincha",     PostalCode = "S2000", IsActive = true };
        db.Localities.AddRange(lPalermo, lNvaCordoba, lPichincha);
        await db.SaveChangesAsync();

        // ── Áreas ─────────────────────────────────────────────────────
        var uHR  = new OrganizationUnit { TenantId = tenant.Id, Name = "Recursos Humanos", Code = "HR"  };
        var uIT  = new OrganizationUnit { TenantId = tenant.Id, Name = "Tecnología",       Code = "IT"  };
        var uOPS = new OrganizationUnit { TenantId = tenant.Id, Name = "Operaciones",      Code = "OPS" };
        db.OrganizationUnits.AddRange(uHR, uIT, uOPS);

        // ── Puestos ───────────────────────────────────────────────────
        var pHR  = new JobPosition { TenantId = tenant.Id, Title = "Analista de RRHH",           Code = "HR-AN"  };
        var pIT  = new JobPosition { TenantId = tenant.Id, Title = "Líder Técnico",               Code = "IT-LT"  };
        var pOPS = new JobPosition { TenantId = tenant.Id, Title = "Coordinador de Operaciones",  Code = "OPS-CO" };
        db.JobPositions.AddRange(pHR, pIT, pOPS);

        await db.SaveChangesAsync();

        // ── 3 Empleados demo ──────────────────────────────────────────
        logger.LogInformation("HumanFlow: creando empleados de demostración...");

        var emp1 = new Employee
        {
            TenantId        = tenant.Id,
            EmployeeNumber  = "EMP-0001",
            FirstName       = "María",
            LastName        = "Gómez",
            Email           = "maria.gomez@demo.local",
            Phone           = "+54 11 5000-0001",
            NationalId      = "28.456.789",
            BirthDate       = new DateOnly(1988, 6, 12),
            MaritalStatus   = MaritalStatus.Married,
            Nationality     = "Argentina",
            AddressStreet   = "Av. Santa Fe 1234",
            CountryId       = gArgentina.Id,
            CityId          = cBsAs.Id,
            LocalityId      = lPalermo.Id,
            Status          = EmployeeStatus.Active,
            HireDate        = new DateOnly(2022, 3, 14),
            JobPositionId   = pHR.Id,
            OrganizationUnitId = uHR.Id
        };
        var emp2 = new Employee
        {
            TenantId        = tenant.Id,
            EmployeeNumber  = "EMP-0002",
            FirstName       = "Lucía",
            LastName        = "Pérez",
            Email           = "lucia.perez@demo.local",
            Phone           = "+54 11 5000-0002",
            NationalId      = "32.100.200",
            BirthDate       = new DateOnly(1993, 11, 3),
            Nationality     = "Argentina",
            AddressStreet   = "Av. Pellegrini 800",
            CountryId       = gArgentina.Id,
            CityId          = cRosario.Id,
            LocalityId      = lPichincha.Id,
            Status          = EmployeeStatus.Active,
            HireDate        = new DateOnly(2021, 9, 1),
            JobPositionId   = pOPS.Id,
            OrganizationUnitId = uOPS.Id
        };
        var emp3 = new Employee
        {
            TenantId        = tenant.Id,
            EmployeeNumber  = "EMP-0003",
            FirstName       = "Tomás",
            LastName        = "Rivas",
            Email           = "tomas.rivas@demo.local",
            Phone           = "+54 11 5000-0003",
            NationalId      = "35.678.901",
            BirthDate       = new DateOnly(1996, 4, 25),
            Nationality     = "Argentina",
            AddressStreet   = "Bv. San Juan 450",
            CountryId       = gArgentina.Id,
            CityId          = cCordoba.Id,
            LocalityId      = lNvaCordoba.Id,
            Status          = EmployeeStatus.Active,
            HireDate        = new DateOnly(2023, 1, 23),
            JobPositionId   = pIT.Id,
            OrganizationUnitId = uIT.Id,
            ManagerEmployeeId  = emp1.Id
        };
        db.Employees.AddRange(emp1, emp2, emp3);

        // Historial de puestos inicial para cada empleado
        db.PositionHistories.AddRange(
            new PositionHistory
            {
                TenantId = tenant.Id, EmployeeId = emp1.Id,
                JobPositionId = pHR.Id, OrganizationUnitId = uHR.Id,
                StartDate = new DateOnly(2022, 3, 14), Reason = PositionChangeReason.Hire
            },
            new PositionHistory
            {
                TenantId = tenant.Id, EmployeeId = emp2.Id,
                JobPositionId = pOPS.Id, OrganizationUnitId = uOPS.Id,
                StartDate = new DateOnly(2021, 9, 1), Reason = PositionChangeReason.Hire
            },
            new PositionHistory
            {
                TenantId = tenant.Id, EmployeeId = emp3.Id,
                JobPositionId = pIT.Id, OrganizationUnitId = uIT.Id,
                ManagerEmployeeId = emp1.Id,
                StartDate = new DateOnly(2023, 1, 23), Reason = PositionChangeReason.Hire
            }
        );

        // ── 3 Contactos demo ──────────────────────────────────────────
        logger.LogInformation("HumanFlow: creando contactos de demostración...");

        db.Contacts.AddRange(
            new Contact
            {
                TenantId         = tenant.Id,
                Kind             = ContactKind.External,
                Type             = ContactType.Legal,
                DisplayName      = "Estudio Legal Demo",
                OrganizationName = "Estudio Legal Demo S.R.L.",
                Email            = "legal@estudiolegal.example",
                Phone            = "+54 11 4000-1000",
                IsActive         = true
            },
            new Contact
            {
                TenantId         = tenant.Id,
                Kind             = ContactKind.External,
                Type             = ContactType.Medical,
                DisplayName      = "Prestador Médico Norte",
                OrganizationName = "PMN Salud S.A.",
                Email            = "rrhh@pmnsalud.example",
                Phone            = "+54 11 4000-2000",
                IsActive         = true
            },
            new Contact
            {
                TenantId    = tenant.Id,
                Kind        = ContactKind.Internal,
                Type        = ContactType.Operational,
                DisplayName = "Mesa de Ayuda Interna",
                Email       = "soporte@demo.local",
                IsActive    = true
            }
        );

        await db.SaveChangesAsync();

        // ── 1 Requisición demo ────────────────────────────────────────
        db.JobRequisitions.Add(new JobRequisition
        {
            TenantId           = tenant.Id,
            RequisitionNumber  = "REQ-001",
            Title              = "Analista de Recursos Humanos",
            JobPositionId      = pHR.Id,
            OrganizationUnitId = uHR.Id,
            RequestedByEmployeeId = emp1.Id,
            Status             = RequisitionStatus.Open,
            Priority           = RequisitionPriority.Normal,
            VacanciesCount     = 1,
            OpenDate           = DateOnly.FromDateTime(DateTime.Today),
            Description        = "Primera requisición de demostración del sistema."
        });

        await db.SaveChangesAsync();

        // ── Usuario administrador ─────────────────────────────────────
        logger.LogInformation("HumanFlow: creando usuario administrador (admin@humanflow.local)...");

        var admin = new ApplicationUser
        {
            UserName        = "admin@humanflow.local",
            Email           = "admin@humanflow.local",
            EmailConfirmed  = true,
            DisplayName     = "Administrador Plataforma",
            PlatformRole    = PlatformRole.PlatformAdministrator
        };

        var result = await userManager.CreateAsync(admin, "HumanFlow123!");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "No se pudo crear el usuario administrador: " +
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation(
            "HumanFlow: datos demo creados — 1 tenant, 3 empleados, 3 contactos, 1 requisición, 1 usuario admin.");
    }
}
