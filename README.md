# SkyPlus

**Sistema de gestión de operaciones para una aerolínea**

SkyPlus es una aplicación desarrollada como proyecto académico para la
materia **Ingeniería de Software I**. El sistema centraliza operaciones
relacionadas con usuarios, vuelos, aeropuertos, aeronaves, pasajeros,
reservas, ventas, reembolsos y procesos de check-in.

La solución está compuesta por una aplicación de escritorio desarrollada
con Windows Forms y una API REST encargada de la lógica de negocio y del
acceso a los datos.

------------------------------------------------------------------------

## Descripción general

SkyPlus fue diseñado siguiendo una arquitectura por capas con el
objetivo de separar responsabilidades y facilitar el mantenimiento y la
evolución del sistema.

La aplicación de escritorio no accede directamente a la base de datos.
Las operaciones se realizan a través de clientes HTTP que consumen los
endpoints expuestos por la API.

``` text
Usuario
   |
   v
SkyPlus.Desktop
   |
   v
Clients HTTP
   |
   v
SkyPlus.API
   |
   v
Services
   |
   v
Entity Framework Core
   |
   v
Base de datos
```

------------------------------------------------------------------------

## Arquitectura de la solución

La solución se encuentra organizada en cuatro proyectos principales:

``` text
SkyPlus
|
+-- SkyPlus.Desktop
|   +-- Models
|   +-- Services
|   +-- Estilos
|   +-- Formularios
|
+-- SkyPlus.API
|   +-- Controllers
|   +-- DTOs
|   +-- Services
|
+-- SkyPlus.Domain
|   +-- Entities
|
+-- SkyPlus.Infrastructure
    +-- Data
```

### SkyPlus.Desktop

Contiene la interfaz de usuario desarrollada con Windows Forms. Los
formularios utilizan clases Client para comunicarse con la API mediante
HTTP.

### SkyPlus.API

Expone los endpoints REST utilizados por Desktop. Contiene los
controladores, DTOs y servicios encargados de procesar las operaciones
solicitadas por los clientes.

### SkyPlus.Domain

Contiene las entidades que representan el dominio del sistema, como
usuarios, vuelos, pasajeros, reservas, asientos, pagos y reembolsos.

### SkyPlus.Infrastructure

Contiene la configuración de persistencia, incluyendo `SkyPlusDbContext`
y la integración con Entity Framework Core.

------------------------------------------------------------------------

## Tecnologías utilizadas

-   C#
-   .NET
-   ASP.NET Core Web API
-   Windows Forms
-   Entity Framework Core
-   SQL Server
-   Swagger / OpenAPI
-   Git
-   GitHub
-   Visual Studio

------------------------------------------------------------------------

## Funcionalidades principales

### Gestión de usuarios

El sistema permite registrar y modificar usuarios, asignar roles,
activar o desactivar cuentas y consultar usuarios mediante filtros.

El acceso a los diferentes módulos depende del rol del usuario
autenticado.

### Gestión de lugares

Permite administrar los aeropuertos utilizados como origen y destino de
los vuelos.

Cada lugar contiene, entre otros datos:

-   Código IATA
-   Nombre
-   Ciudad
-   País

### Gestión de aeronaves

Permite administrar las aeronaves disponibles en el sistema mediante su
matrícula y modelo.

### Gestión de vuelos

Permite crear, modificar, consultar y administrar vuelos.

Cada vuelo contiene información como:

-   Número de vuelo
-   Aeronave
-   Lugar de origen
-   Lugar de destino
-   Fecha y hora de salida
-   Fecha y hora de llegada
-   Estado
-   Tarifa
-   Usuario operador

Los estados utilizados actualmente son:

-   Programado
-   Confirmado
-   Cancelado
-   Finalizado

Al crear un nuevo vuelo, el sistema genera automáticamente **180
asientos**, distribuidos en 30 filas y seis posiciones por fila (`A` a
`F`). Los asientos se crean inicialmente con estado `Libre`.

### Gestión de pasajeros

Permite registrar y administrar los pasajeros que posteriormente pueden
ser asociados a una reserva.

### Gestión de reservas

El flujo de creación de una reserva permite:

1.  Buscar y seleccionar un pasajero.
2.  Registrar un nuevo pasajero durante el proceso si todavía no existe.
3.  Seleccionar un lugar de origen.
4.  Seleccionar un destino disponible.
5.  Consultar los vuelos correspondientes a la ruta elegida.
6.  Seleccionar un vuelo.
7.  Consultar los asientos asociados al vuelo.
8.  Seleccionar un asiento disponible.
9.  Visualizar la tarifa correspondiente.
10. Confirmar la reserva.

El módulo también permite consultar y cancelar reservas existentes.

### Ventas y reembolsos

El proyecto incluye interfaces para los módulos de ventas y reembolsos.
Su integración funcional completa continúa formando parte del desarrollo
incremental del sistema.

### Check-in y Boarding Pass

El sistema contempla módulos destinados al proceso de check-in y a la
gestión de tarjetas de embarque.

### Reportes

Módulo destinado a la consulta de información y estadísticas del
sistema.

### Configuración

El sistema dispone de un módulo de configuración. Las opciones
disponibles dependen del rol del usuario. Los usuarios no
administradores pueden acceder al apartado de seguridad correspondiente.

------------------------------------------------------------------------

## Roles y permisos

El Dashboard habilita los módulos según el rol del usuario autenticado.

  -----------------------------------------------------------------------
  Rol                                 Módulos principales
  ----------------------------------- -----------------------------------
  Administrador                       Usuarios, Vuelos, Lugares,
                                      Aeronaves, Reportes, Configuración

  Gerente                             Usuarios, Vuelos, Lugares,
                                      Aeronaves, Reportes, Configuración

  Agente de Reservas                  Pasajeros, Reservas, Ventas,
                                      Cancelaciones, Reembolsos,
                                      Configuración

  Agente de Check-in                  Check-in, Boarding Pass,
                                      Configuración
  -----------------------------------------------------------------------

Los permisos y funcionalidades continúan evolucionando de acuerdo con
los requerimientos de las distintas entregas.

------------------------------------------------------------------------

## Configuración del entorno

### Requisitos

Para ejecutar el proyecto se recomienda contar con:

-   Visual Studio con soporte para desarrollo .NET.
-   SDK de .NET correspondiente a la solución.
-   SQL Server disponible localmente o en un servidor accesible.
-   Git para clonar y mantener actualizado el repositorio.

------------------------------------------------------------------------

## Configuración de la base de datos

Antes de ejecutar la API es necesario configurar la conexión a la base
de datos utilizada por SkyPlus.

### 1. Localizar la configuración de conexión

Dentro del proyecto `SkyPlus.API`, revisar el archivo:

``` text
SkyPlus.API/appsettings.json
```

La configuración normalmente contiene una sección similar a:

``` json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVIDOR;Database=SkyPlus;User Id=USUARIO;Password=CONTRASEÑA;TrustServerCertificate=True;"
  }
}
```

> El nombre exacto de la cadena de conexión debe coincidir con el
> utilizado por `SkyPlusDbContext` en la configuración de la API.

### 2. Modificar los datos para el entorno local

Cada integrante del equipo debe adaptar la cadena de conexión a su
propia instalación de SQL Server.

Los valores que normalmente deben modificarse son:

  -----------------------------------------------------------------------
  Valor                               Qué configurar
  ----------------------------------- -----------------------------------
  `Server`                            Nombre o dirección de la instancia
                                      local de SQL Server

  `Database`                          Nombre de la base de datos
                                      utilizada por SkyPlus

  `User Id`                           Usuario de SQL Server, si se
                                      utiliza autenticación SQL

  `Password`                          Contraseña correspondiente al
                                      usuario anterior

  `TrustServerCertificate`            Puede mantenerse en `True` para el
                                      entorno local cuando corresponda
  -----------------------------------------------------------------------

Por ejemplo, si la instancia local es `localhost`, la base se llama
`SkyPlus`, el usuario es `sa` y se utiliza autenticación SQL, la
estructura sería:

``` text
Server=localhost;Database=SkyPlus;User Id=sa;Password=TU_CONTRASEÑA;TrustServerCertificate=True;
```

La contraseña del ejemplo debe reemplazarse por la correspondiente al
entorno local.

### 3. Autenticación integrada de Windows

Si SQL Server está configurado para utilizar autenticación de Windows,
la cadena puede utilizar una estructura similar a:

``` text
Server=SERVIDOR;Database=SkyPlus;Trusted_Connection=True;TrustServerCertificate=True;
```

En ese caso no es necesario indicar `User Id` ni `Password`.

### 4. Credenciales y seguridad

No se deben subir contraseñas reales, credenciales personales ni otros
secretos al repositorio.

Si el proyecto utiliza `appsettings.json` compartido entre integrantes,
se recomienda mantener allí únicamente configuración no sensible y
almacenar las credenciales locales mediante mecanismos de configuración
apropiados para desarrollo, como User Secrets.

Cada integrante debe verificar que la API esté utilizando su propia
configuración antes de ejecutarla.

### 5. Verificar la conexión

Una vez configurada la cadena:

1.  Ejecutar `SkyPlus.API`.
2.  Confirmar que la aplicación inicia sin errores de conexión.
3.  Abrir Swagger.
4.  Ejecutar un endpoint de consulta, por ejemplo un `GET`.
5.  Verificar que la API pueda recuperar información de la base de
    datos.

Si la API inicia pero los endpoints devuelven errores relacionados con
la conexión, revisar principalmente el nombre del servidor, nombre de la
base, usuario, contraseña y permisos del usuario configurado.

------------------------------------------------------------------------

## Ejecución del proyecto

### 1. Clonar el repositorio

``` bash
git clone <URL_DEL_REPOSITORIO>
```

### 2. Abrir la solución

Abrir el archivo:

``` text
SkyPlus.sln
```

con Visual Studio.

### 3. Configurar la conexión

Adaptar la configuración de base de datos del proyecto `SkyPlus.API`
siguiendo las instrucciones de la sección anterior.

### 4. Ejecutar la API

Iniciar el proyecto:

``` text
SkyPlus.API
```

Swagger permite consultar y probar los endpoints disponibles.

### 5. Ejecutar Desktop

Con la API en funcionamiento, iniciar:

``` text
SkyPlus.Desktop
```

La aplicación Desktop utilizará sus clases Client para comunicarse con
la API.

------------------------------------------------------------------------

## Pruebas

Durante el desarrollo se realizan pruebas de los endpoints mediante
Swagger y pruebas de integración desde la aplicación Desktop.

Se verifican operaciones HTTP como:

-   GET
-   POST
-   PUT
-   DELETE

El objetivo de las pruebas de integración es comprobar el flujo
completo:

``` text
Desktop -> API -> Services -> Entity Framework Core -> Base de datos
```

También se verifica el comportamiento inverso al recuperar y mostrar los
datos en los formularios.

------------------------------------------------------------------------

## Control de versiones

El proyecto utiliza Git y GitHub para el trabajo colaborativo.

Flujo básico:

``` bash
git status
git add .
git commit -m "Descripción del cambio"
git push
```

Para funcionalidades nuevas se recomienda trabajar en ramas separadas y
mantener actualizada la rama local antes de integrar cambios realizados
por otros integrantes.

------------------------------------------------------------------------

## Estado del proyecto

SkyPlus se encuentra en desarrollo activo. Las funcionalidades se
implementan de manera incremental de acuerdo con los requisitos y
entregas definidos para el proyecto académico.

Algunos módulos se encuentran completamente conectados a la API,
mientras que otros poseen interfaces preparadas para continuar su
implementación en próximas iteraciones.

------------------------------------------------------------------------

## Contexto académico

Proyecto desarrollado para la materia **Ingeniería de Software I**.

El desarrollo busca aplicar conceptos relacionados con:

-   Ingeniería de requisitos
-   Historias de usuario
-   Requisitos funcionales
-   Arquitectura por capas
-   Diseño de software
-   Persistencia de datos
-   APIs REST
-   Interfaces de usuario
-   Pruebas
-   Control de versiones
-   Trabajo colaborativo

------------------------------------------------------------------------

## Equipo

Proyecto desarrollado de forma colaborativa por estudiantes de
Ingeniería de Software I.

------------------------------------------------------------------------

## Licencia

Proyecto desarrollado con fines académicos y educativos.
