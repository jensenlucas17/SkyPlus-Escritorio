
# Recuperación de MariaDB - SkyPlus

## Cuándo utilizar este procedimiento

Cuando RestoreWorker se interrumpe o informa:
- ReadOnlyRecoveryRequired
- EnablingReadOnly
- ReadOnlyEnabled
- DisablingReadOnly

No reanudar SkyPlus hasta completar la revisión.

## 1. Mantener SkyPlus detenido

- No iniciar SkyPlus.API.
- No iniciar SkyPlus.Desktop.
- Mantener el archivo mantenimiento.lock.
- No ejecutar nuevamente RestoreWorker.

## 2. Consultar el estado real de MariaDB

Conectarse desde DBeaver utilizando la cuenta administrativa
de recuperación y ejecutar:

SHOW GLOBAL VARIABLES LIKE 'read_only';

## 3. Recuperar el servidor

Solo después de confirmar que no hay una restauración
en ejecución y que es seguro reanudar las escrituras:

SET GLOBAL read_only = OFF;

Verificar nuevamente:

SHOW GLOBAL VARIABLES LIKE 'read_only';

El resultado debe indicar OFF.

## 4. Revisar la operación interrumpida

Consultar:

C:\SkyPlus\Auditoria\restore-state.json

Revisar:
- OperationId
- OperationType
- Phase
- ReadOnlyActivated
- RestorationStarted
- RestorationCompleted
- Error

IMPORTANTE:
Si RestorationStarted = true, no basta con desactivar
read_only. La base de datos podría estar parcialmente
restaurada y necesita validación o recuperación.

## 5. Finalización

- Registrar el incidente y las acciones realizadas.
- Confirmar la integridad y disponibilidad de la base.
- Resolver administrativamente el estado pendiente.
- Reanudar SkyPlus únicamente después de la validación.

Nunca eliminar restore-state.json para saltar
una operación pendiente.
