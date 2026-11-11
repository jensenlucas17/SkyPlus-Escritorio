# Cuentas de demostración de SkyPlus

Estas cuentas se crean al ejecutar `database/seed_demo.sql`.
Son datos ficticios destinados a pruebas y presentaciones.

**Contraseña de todas las cuentas:** `SkyPlusDemo2026!`

| Rol | Correo electrónico | Estado |
| --- | --- | --- |
| Administrador | admin@skyplus.test | Activo |
| Segundo administrador | admin2@skyplus.test | Activo |
| Gerente | gerente@skyplus.test | Activo |
| Agente de Reservas | reservas@skyplus.test | Activo |
| Segundo agente de reservas | reservas2@skyplus.test | Activo |
| Agente de Check-in | checkin@skyplus.test | Activo |
| Agente de Reservas — prueba de cuenta inactiva | inactivo@skyplus.test | Inactivo |

## Uso sugerido

- Administrador: usuarios, catálogos, vuelos, reportes y backups.
- Segundo administrador: segunda autorización de restauración.
- Gerente: gestión operativa, reportes y consulta de backups.
- Agente de Reservas: pasajeros, reservas, ventas y reembolsos.
- Agente de Check-in: equipajes, adicionales, check-in y embarque.
- Cuenta inactiva: comprobar que el sistema rechaza el acceso.

La contraseña publicada es exclusivamente de demostración.
No reutilizar estas cuentas o contraseñas en una instalación real.

## Preparación

1. Preparar el esquema de la base mediante las migraciones.
2. Guardar un respaldo si existen datos que se necesiten conservar.
3. Cerrar Desktop, la API y los workers.
4. Ejecutar completo `database/seed_demo.sql` sobre la base de prueba.
5. Iniciar la API y Desktop.
6. Ingresar con una de las cuentas activas.

El script reemplaza los datos existentes. No debe ejecutarse sobre
una base con información real que deba conservarse.

## Datos para la presentación

El script utiliza la fecha de ejecución como referencia y genera:

- Dos períodos históricos consecutivos de 14 días.
- Rutas con distintas cantidades de vuelos y ocupación.
- Reservas y operaciones económicas para consultar reportes.
- Vuelos próximos para nuevas ventas.
- Los vuelos SK0085 y SK0086, con salida dos y cinco horas
  después de ejecutar el script, para pruebas operativas.

Al terminar, el script muestra las fechas que se pueden seleccionar
para comparar los períodos.

Las reservas pendientes de pago vencen a los cinco minutos.
Los datos pueden cambiar al utilizar la aplicación.