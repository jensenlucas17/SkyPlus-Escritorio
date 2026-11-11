# SkyPlus

Sistema de gestión de operaciones de una aerolínea, desarrollado como proyecto académico para Ingeniería de Software I.

SkyPlus permite administrar vuelos, pasajeros, reservas, pagos, equipajes, check-in y embarque. Incluye reportes gerenciales y herramientas de respaldo y restauración de la base de datos.

Es una aplicación de uso interno del personal, con cuatro roles: Administrador, Gerente, Agente de Reservas y Agente de Check-in.

## Documentación

| Documento | PDF | Word editable |
| --- | --- | --- |
| Especificación de Requisitos de Software | [Consultar ERS](docs/ERS_SkyPlus.pdf) | [Descargar Word](docs/ERS_SkyPlus.docx) |
| Manual de usuario | [Consultar manual](docs/Manual_Usuario_SkyPlus.pdf) | [Descargar Word](docs/Manual_Usuario_SkyPlus.docx) |

La ERS describe el alcance actual, las reglas de negocio y las funcionalidades pendientes. El manual explica los procedimientos de uso según el rol.

## Funcionalidades principales

- Administración de usuarios, aeronaves y aeropuertos.
- Gestión de vuelos, disponibilidad de aeronaves y distribución de asientos.
- Registro de pasajeros y venta de pasajes con equipaje opcional.
- Validación de disponibilidad de asientos y viajes superpuestos.
- Registro de pagos, cancelaciones y reembolsos.
- Pesaje de equipajes, cobro de adicionales, check-in y embarque.
- Generación de boarding pass en PDF con código QR.
- Reportes con filtros, gráficos, ranking de rutas y comparación de períodos.
- Exportación de resúmenes y tablas principales a PDF y Excel.
- Copias de seguridad y restauración con controles de autorización.

## Arquitectura

La aplicación de escritorio consume una API REST. El servidor valida permisos y reglas de negocio y accede a MariaDB mediante Entity Framework Core.

```text
Desktop — Windows Forms
          |
          | HTTPS / JSON / JWT
          v
API — ASP.NET Core
          |
          v
Servicios de negocio
          |
          v
Entity Framework Core
          |
          v
MariaDB
```

Desktop no accede directamente a la base de datos.

### Proyectos de la solución

| Proyecto | Responsabilidad |
| --- | --- |
| `SkyPlus.Desktop` | Interfaz Windows Forms, clientes HTTP, gráficos y exportaciones. |
| `SkyPlus.API` | Endpoints, autenticación, autorización y reglas de negocio. |
| `SkyPlus.Domain` | Entidades y contratos compartidos. |
| `SkyPlus.Infrastructure` | Contexto de datos, mapeos y migraciones. |
| `SkyPlus.BackupWorker` | Generación de un backup automático por ejecución. |
| `SkyPlus.RestoreWorker` | Verificaciones y procedimientos de restauración. |
| `SkyPlus.RestoreLauncher` | Coordinación de la restauración en Windows. |
| `SkyPlus.RestoreSecurityTests` | Pruebas de seguridad de restauración. |

## Tecnologías

- C# y .NET 10.
- Windows Forms y ASP.NET Core.
- Entity Framework Core 9, Pomelo y MariaDB.
- JWT para autenticación y autorización por roles.
- PBKDF2 para almacenamiento de contraseñas.
- Swagger para consulta y prueba de endpoints.
- QuestPDF, ClosedXML y QRCoder.
- Git y GitHub.

## Preparación del entorno

### Requisitos

- Windows compatible con .NET 10.
- SDK de .NET 10.
- Visual Studio con soporte para Windows Forms y ASP.NET Core, o herramientas equivalentes.
- MariaDB disponible.
- Git.
- Certificado HTTPS de desarrollo confiable.

Los componentes de backup y restauración requieren además las herramientas de MariaDB, carpetas, claves y archivos de credenciales configurados por el responsable técnico.

### 1. Obtener el proyecto

```powershell
git clone https://github.com/jensenlucas17/SkyPlus-Escritorio.git
cd SkyPlus-Escritorio
dotnet restore SkyPlus.sln
```

### 2. Configurar la API

La configuración base se encuentra en:

```text
SkyPlus.API/appsettings.json
```

La instalación local debe proporcionar:

| Clave | Contenido |
| --- | --- |
| `ConnectionStrings:SkyPlusConnection` | Conexión a la base MariaDB. |
| `Jwt:Key` | Clave aleatoria de firma codificada en Base64. |
| `Jwt:Issuer` | Emisor del token. |
| `Jwt:Audience` | Audiencia del token. |
| `Jwt:ExpirationMinutes` | Duración de la sesión autenticada. |

Para valores sensibles, utilizá **Administrar secretos de usuario** sobre `SkyPlus.API` en Visual Studio o variables de entorno.

No subas contraseñas, claves privadas ni archivos de credenciales al repositorio.

### 3. Preparar la base y los datos iniciales

El modelo y las migraciones se encuentran en `SkyPlus.Infrastructure`.

Para aplicar las migraciones se necesita una herramienta `dotnet-ef` compatible con EF Core 9. Con la conexión configurada:

```powershell
dotnet ef database update `
  --project SkyPlus.Infrastructure `
  --startup-project SkyPlus.API
```

Verificá el servidor y la base de destino antes de ejecutar el comando.

**Aplicar las migraciones no garantiza que existan cuentas para ingresar.**

Después de aplicar las migraciones, se puede cargar el conjunto
de demostración disponible en `database/seed_demo.sql`.

El script crea los roles, cuentas y registros necesarios para
probar los principales módulos. Las credenciales están documentadas
en [Accesos de demostración](docs/ACCESOS_DEMO.md).

**El script reemplaza todos los datos de negocio de `skyplus`.**
Antes de ejecutarlo, guardá un respaldo y cerrá Desktop, la API
y los workers. Utilizalo únicamente sobre una base de pruebas.

En DBeaver, abrí el archivo en un editor SQL conectado al servidor
correspondiente y ejecutá el script completo con Alt + X.

El script necesita que el esquema ya exista y conserva el historial
de migraciones.

### 4. Preparar HTTPS y compilar

```powershell
dotnet dev-certs https --trust
dotnet build SkyPlus.sln
```

Los pasos de instalación deben verificarse en el equipo Windows de destino, incluyendo conexión, datos iniciales y certificado.

## Ejecución

Primero iniciá la API:

```powershell
dotnet run --project SkyPlus.API --launch-profile https
```

El perfil de desarrollo utiliza:

```text
HTTPS:  https://localhost:7198
HTTP:   http://localhost:5004
Swagger: https://localhost:7198/swagger
```

Después iniciá Desktop:

```powershell
dotnet run --project SkyPlus.Desktop
```

También pueden configurarse ambos proyectos como proyectos de inicio en Visual Studio.

Desktop apunta actualmente a `https://localhost:7198/`. Para utilizar varias estaciones, es necesario configurar el endpoint, los certificados y el acceso al servidor.

## Roles y permisos

| Rol | Funciones principales |
| --- | --- |
| Administrador | Usuarios, vuelos, catálogos, reportes y acciones privilegiadas de backups y restauración. |
| Gerente | Vuelos, catálogos, reportes y consulta de backups. |
| Agente de Reservas | Pasajeros, reservas, ventas, cancelaciones y reembolsos. |
| Agente de Check-in | Equipajes, cobros adicionales, check-in, boarding pass y embarque. |

Los permisos se comprueban en la API. El acceso a un módulo no implica autorización para todas las operaciones del sistema.

## Pruebas

Para ejecutar el runner de seguridad de restauración:

```powershell
dotnet run --project SkyPlus.RestoreSecurityTests
```

El resultado esperado para sus 39 comprobaciones es:

```text
Pruebas aprobadas: 39
Pruebas fallidas:  0
```

Estas comprobaciones no sustituyen las pruebas de interfaz, ventas, concurrencia ni restauraciones reales en el entorno definitivo. Los escenarios de aceptación están documentados en la ERS.

## Alcance y limitaciones

- El sistema registra pagos y reembolsos, pero no ejecuta operaciones bancarias ni se integra directamente con un POS.
- El flujo de venta utiliza varias solicitudes; no constituye una única transacción global.
- No incluye operación offline, envío de correos ni actualización en vivo de todos los listados.
- Quedan pendientes el bloqueo por intentos fallidos, el cierre por inactividad, 2FA y la unificación de la política de contraseña inicial.
- La comparación de períodos está disponible para Vuelos y utiliza los estados actuales de los registros.
- Las exportaciones tienen limitaciones de contenido y deben generarse nuevamente después de cambiar filtros.

La ERS detalla las restricciones y mejoras futuras. Los procedimientos de restauración deben ejecutarse únicamente con el entorno y las autorizaciones correspondientes.

## Contexto académico

Proyecto desarrollado para Ingeniería de Software II, aplicando relevamiento de requisitos, arquitectura por capas, persistencia, reglas de negocio, control de acceso, pruebas y documentación.

La documentación y el código deben actualizarse juntos cuando cambie el alcance de una entrega.