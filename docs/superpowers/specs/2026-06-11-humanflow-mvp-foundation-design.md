# HumanFlow MVP Fundacional - Diseno

## Objetivo

HumanFlow sera una plataforma web SaaS multi-tenant para gestionar procesos de recursos humanos. El primer alcance no intentara cubrir todo el ciclo de vida del empleado. Se enfocara en construir una base expansible: administracion de plataforma, administracion de tenants, seguridad, roles, permisos, legajo fundacional del empleado, contactos internos y externos, y datos maestros.

El diseno debe permitir crecer despues hacia reclutamiento, entrevistas, onboarding, carrera, evaluaciones, licencias, compensaciones y salida del empleado sin rehacer la arquitectura base.

## Decisiones Aprobadas

- Interfaces web con Blazor Web App.
- MVP inicial con interactividad del lado servidor.
- Stack backend en Microsoft .NET y C#.
- Desarrollo local sobre SQL Server Express, LocalDB o Developer Edition.
- Migracion futura a Azure SQL Database.
- Arquitectura multi-tenant completa.
- Modelo de datos multi-tenant hibrido.
- Usuarios con modelo mixto: usuarios de tenant, usuarios multi-tenant y administradores globales de plataforma.
- Autenticacion mixta: local inicialmente, preparada para SSO por tenant.
- Autorizacion hibrida: roles base del sistema, roles personalizados por tenant y permisos granulares.
- Legajo fundacional expansible.

## Enfoque Arquitectonico

El sistema comenzara como un monolito modular en .NET. Esta decision reduce complejidad operativa inicial, mantiene alta productividad y evita partir prematuramente un dominio que todavia esta tomando forma.

La solucion se organizara por modulos funcionales con limites claros:

- Plataforma: tenants, planes, configuracion global, administradores globales y auditoria global.
- Identidad y acceso: usuarios, autenticacion, membresias, roles, permisos y politicas.
- Administracion de tenant: configuracion propia del tenant, datos maestros, unidades organizativas y parametros.
- Legajo: empleados, datos personales, datos laborales, estado, relacion jerarquica, documentos y eventos historicos.
- Contactos: contactos internos, externos y relaciones con empleados, areas o tenants.
- Auditoria: trazabilidad transversal de cambios relevantes.

Aunque el primer despliegue sea monolitico, los modulos deben exponer servicios internos bien definidos para poder abrir APIs o extraer componentes en el futuro si el producto lo justifica.

## Multi-Tenancy

El sistema usara un modelo hibrido:

- Tenants estandar: almacenados en una base compartida con columna `TenantId` en entidades tenant-scoped.
- Tenants grandes o regulados: podran operar en una base dedicada.
- Catalogo central: mantendra la informacion de cada tenant y su estrategia de almacenamiento.

El tenant se resolvera por subdominio, dominio personalizado o contexto de usuario autenticado. La resolucion de tenant sera una responsabilidad central del sistema y debera estar disponible antes de acceder a datos tenant-scoped.

Las consultas de entidades tenant-scoped deben aplicar aislamiento por tenant de forma sistematica. Para bases compartidas, el diseno debe combinar filtros globales de aplicacion, restricciones de modelo y controles defensivos en base de datos cuando corresponda.

## Identidad y Acceso

La autenticacion local sera el primer mecanismo disponible: email, password, recuperacion de acceso y politicas basicas de seguridad. El modelo debe quedar preparado para agregar proveedores externos por tenant, especialmente Microsoft Entra ID.

Tipos de usuario:

- Usuario de tenant: opera dentro de un solo tenant.
- Usuario multi-tenant: tiene membresias en varios tenants, con roles independientes por tenant.
- Administrador global de plataforma: administra tenants, configuracion global, soporte, planes, auditoria y operacion SaaS.

La autorizacion separara permisos de plataforma y permisos de tenant. Un administrador global no debe convertirse implicitamente en administrador funcional de todos los tenants salvo que una accion de soporte controlada lo habilite y quede auditada.

## Roles y Permisos

El modelo de autorizacion sera hibrido:

- Roles base del sistema: definidos por la plataforma para acelerar configuracion inicial.
- Roles personalizados: definidos por cada tenant.
- Permisos granulares: acciones concretas sobre modulos y recursos.

Ejemplos de roles base:

- Administrador de plataforma.
- Operador de soporte de plataforma.
- Administrador de tenant.
- Administrador de RRHH.
- Analista de RRHH.
- Supervisor.
- Empleado.
- Auditor.

Los permisos deben modelarse como capacidades, por ejemplo `employees.read`, `employees.create`, `employees.update`, `employees.documents.manage`, `contacts.manage`, `roles.assign` y `tenant.settings.manage`.

## Legajo Fundacional

El legajo inicial sera expansible, no exhaustivo. Debe cubrir lo necesario para administrar empleados y dejar puntos de extension para modulos futuros.

Alcance inicial:

- Identificacion del empleado.
- Datos personales principales.
- Datos laborales principales.
- Estado laboral.
- Fecha de ingreso y fecha de salida cuando aplique.
- Area, puesto y responsable.
- Contactos asociados.
- Documentos como metadata.
- Eventos historicos genericos.

Los eventos historicos funcionaran como una linea de tiempo auditable para registrar cambios importantes sin crear todavia todos los modulos especializados. Ejemplos: alta, cambio de puesto, cambio de area, cambio de responsable, actualizacion documental, baja o nota administrativa.

## Contactos Internos y Externos

El sistema contemplara contactos internos y externos desde el inicio.

Contactos internos:

- Empleados u otros usuarios vinculados al tenant.
- Relaciones como responsable, referente de RRHH, aprobador o contacto operativo.

Contactos externos:

- Personas ajenas al tenant que interactuan con RRHH.
- Ejemplos: consultores, proveedores, abogados, prestadores medicos, referencias laborales o contactos de emergencia.

El contacto externo debe ser reutilizable y relacionable con empleados, areas, procesos futuros o entidades del tenant.

## Datos Maestros

El MVP incluira datos maestros necesarios para operar el legajo y la administracion:

- Tenants.
- Unidades organizativas.
- Areas o departamentos.
- Puestos.
- Tipos de documento.
- Estados de empleado.
- Tipos de contacto.
- Paises, provincias o localidades si el alcance inicial lo requiere.

Los datos maestros deben ser tenant-scoped salvo los catalogos globales estrictamente compartidos por la plataforma.

## Base de Datos

El desarrollo local podra usar SQL Server Express, LocalDB o Developer Edition. La arquitectura debe mantenerse compatible con Azure SQL Database.

Restricciones para conservar portabilidad:

- No depender de SQL Server Agent.
- No usar linked servers.
- No usar nombres de tres o cuatro partes entre bases.
- No depender de rutas fisicas del servidor.
- No usar `USE` para cambiar contexto de base en codigo de aplicacion.
- No colocar archivos binarios grandes en la base como estrategia principal.

La metadata de documentos vivira en SQL. Los archivos deben almacenarse fuera de la base, con una abstraccion que permita usar Azure Blob Storage en cloud.

## Auditoria

Las entidades sensibles deben registrar:

- Tenant.
- Fecha de creacion.
- Usuario creador.
- Fecha de modificacion.
- Usuario modificador.
- Estado activo/inactivo o borrado logico cuando corresponda.

Ademas, los cambios significativos deben generar eventos de auditoria o timeline. La auditoria debe permitir responder quien hizo que cambio, cuando y en que tenant.

## Interfaz Web

La UI sera Blazor Web App, con enfoque administrativo y B2B. Debe priorizar claridad, densidad razonable, busqueda, filtros y flujos repetibles por encima de composiciones de marketing.

Primeras areas de UI:

- Shell de plataforma.
- Selector de tenant para usuarios multi-tenant.
- Administracion de tenants.
- Administracion de usuarios.
- Roles y permisos.
- Datos maestros.
- Listado de empleados.
- Ficha de empleado.
- Contactos.

## Errores y Seguridad Operativa

Los errores de acceso deben distinguir:

- Usuario no autenticado.
- Usuario autenticado sin tenant activo.
- Usuario sin permisos suficientes.
- Recurso inexistente.
- Recurso existente pero no visible para el tenant actual.

El sistema no debe revelar informacion de otros tenants mediante mensajes de error, busquedas, IDs predecibles o endpoints administrativos.

## Testing y Validacion

El MVP debe incluir pruebas orientadas a riesgos principales:

- Resolucion correcta de tenant.
- Aislamiento de datos entre tenants.
- Autorizacion por permiso.
- Usuarios multi-tenant con roles diferentes por tenant.
- Administradores globales sin acceso funcional automatico a datos tenant.
- CRUD basico de empleados.
- CRUD de contactos.
- Datos maestros por tenant.

Las pruebas de aislamiento multi-tenant son obligatorias desde el inicio.

## Fuera de Alcance del MVP

No se implementaran en el primer alcance:

- Reclutamiento completo.
- Portal de candidatos.
- Entrevistas.
- Evaluaciones de desempeno.
- Nomina/liquidacion de sueldos.
- Licencias y ausencias completas.
- Workflows avanzados.
- Firma digital.
- App movil.
- Microservicios.

El diseno debe dejar espacio para estos modulos sin incluirlos todavia.

## Criterios de Exito

El MVP fundacional sera exitoso si permite:

- Crear y administrar tenants.
- Crear usuarios locales.
- Asignar usuarios a uno o varios tenants.
- Diferenciar administradores globales de usuarios de tenant.
- Crear roles base y roles personalizados.
- Asignar permisos granulares.
- Administrar datos maestros iniciales.
- Crear y consultar legajos de empleados.
- Registrar contactos internos y externos.
- Mantener aislamiento entre tenants.
- Ejecutar localmente con SQL Server y desplegar luego sobre Azure SQL Database con cambios minimos.
