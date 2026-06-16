# HumanFlow

HRMS (Human Resources Management System) multi-tenant construido con **.NET 10** y **Blazor Server**. Cubre el ciclo de vida del empleado, evaluaciones de desempeño, reclutamiento, datos maestros de geografía y compensaciones.

> Este README se va actualizando a medida que se agregan nuevas funcionalidades. La sección [Funcionalidades](#funcionalidades) refleja el estado actual del proyecto fase por fase.

## Stack tecnológico

- **.NET 10** / **C# 13**
- **Blazor Server** (`@rendermode InteractiveServer`)
- **Entity Framework Core 10** + **SQL Server** (LocalDB en desarrollo)
- **ASP.NET Core Identity** con login externo (Microsoft / Google) opcional
- Arquitectura en capas: `Domain` → `Application` → `Infrastructure` → `Web`

## Estructura del proyecto

```
HumanFlow.slnx
src/
  HumanFlow.Domain/          Entidades y enums del dominio (Employees, Recruitment, Geography, Tenancy, Security...)
  HumanFlow.Application/     Capa de aplicación (casos de uso) — en construcción
  HumanFlow.Infrastructure/  Implementaciones de infraestructura — en construcción
  HumanFlow.Web/             Blazor Server: páginas, DbContext, migraciones, seed de datos
tests/
  HumanFlow.Tests/           Pruebas unitarias
docs/                        Specs y planes de implementación
```

Actualmente la lógica de acceso a datos vive directamente en las páginas Razor de `HumanFlow.Web` a través de `ApplicationDbContext` (EF Core), sin pasar todavía por `Application`/`Infrastructure`.

## Funcionalidades

| Fase | Contenido | Estado |
|------|-----------|--------|
| A | Ciclo de vida del empleado (alta, legajo, estados) + design system | ✅ |
| B | Evaluaciones de desempeño (performance reviews) | ✅ |
| C | Pipeline de reclutamiento completo (requisiciones, candidatos, postulaciones, entrevistas) | ✅ |
| D | Datos maestros de geografía (País → Ciudad → Localidad) e integración en Legajo/Contactos | ✅ |
| E | Historial de compensaciones (salarios) | ✅ |
| F | Licencias y ausencias | ⏳ Pendiente |
| G | Documentación del empleado | ⏳ Pendiente |
| H | Dashboard | ⏳ Pendiente |
| I | Reportes y exportación | ⏳ Pendiente |

### Módulos disponibles hoy

- **Legajos** (`/employees`) — alta y ficha de empleados, datos personales, domicilio y lugar de nacimiento (con selects geográficos en cascada País → Ciudad → Localidad), historial de puesto y de compensaciones.
- **Reclutamiento** (`/recruitment`) — requisiciones de personal, candidatos, postulaciones y entrevistas.
- **Contactos** (`/contacts`) — contactos de emergencia / referencias del empleado.
- **Datos maestros** (`/master-data`) — CRUD de catálogos: países, ciudades, localidades, áreas (`OrganizationUnit`) y puestos (`JobPosition`).
- **Administración** (`/admin`) — gestión de usuarios, roles y tenants.

## Multi-tenant

Todas las entidades de negocio implementan `ITenantScoped` (propiedad `TenantId`). Las consultas a datos maestros y de empleados se filtran siempre por tenant; al resolver "el tenant actual" en ausencia de autenticación multi-tenant completa, se usa el primer tenant creado (`Tenants.OrderBy(t => t.CreatedAt).First()`) para garantizar un resultado determinístico.

## Requisitos previos

- [.NET SDK 10.0.300+](https://dotnet.microsoft.com/download) (ver [global.json](global.json))
- SQL Server LocalDB (incluido con Visual Studio) o una instancia de SQL Server accesible
- (Opcional) Credenciales OAuth de Microsoft/Google si se quiere habilitar login externo

## Configuración inicial

1. Cloná el repositorio y restaurá los paquetes:
   ```bash
   dotnet restore
   ```
2. Verificá la cadena de conexión en [src/HumanFlow.Web/appsettings.json](src/HumanFlow.Web/appsettings.json) (por defecto apunta a `localhost\HumanFlow` con autenticación de Windows).
3. **Nunca** completes `ClientId`/`ClientSecret` en `appsettings.json` ni los commitees. Configurá credenciales de login externo (si las necesitás) vía `dotnet user-secrets`:
   ```bash
   dotnet user-secrets set "Authentication:Microsoft:ClientId" "..." --project src/HumanFlow.Web
   dotnet user-secrets set "Authentication:Microsoft:ClientSecret" "..." --project src/HumanFlow.Web
   ```
4. Ejecutá la aplicación:
   ```bash
   dotnet run --project src/HumanFlow.Web/HumanFlow.Web.csproj --launch-profile https
   ```

La base de datos, las migraciones pendientes y los datos demo (tenant, países/ciudades/localidades, empleados de ejemplo) se crean/aplican automáticamente al iniciar (`StartupInitializer`), no se requiere correr `dotnet ef database update` manualmente en desarrollo.

## Tests

```bash
dotnet test
```

## Convenciones técnicas

- **Soft delete**: las entidades se marcan con `IsDeleted = true` + `UpdatedAt`; nunca se usa `DbContext.Remove()` en código de aplicación.
- **Resolución de tenant**: siempre ordenada por `CreatedAt` para evitar resultados no determinísticos cuando existe más de un tenant.
- **Selects en cascada** (`Country` → `City` → `Locality`): se deshabilitan con `disabled="@(!Guid.TryParse(...))"` hasta que el padre tenga un valor válido, y se resetean los hijos con `@bind:after`.
- **Secretos**: credenciales (OAuth, connection strings sensibles) solo vía `dotnet user-secrets`, jamás en código o `appsettings.json`.
