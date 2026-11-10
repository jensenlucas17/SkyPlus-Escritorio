# SkyPlus — restauración automática desde Desktop

## Archivos modificados

- `SkyPlus.API/Program.cs`: registra el servicio local de apagado.
- `SkyPlus.API/Services/LocalShutdownService.cs` (**nuevo**): escucha un canal Windows Named Pipe restringido al usuario actual; usa un token aleatorio local por ejecución y `StopApplication()` para terminar ordenadamente la API. No crea un endpoint HTTP.
- `SkyPlus.RestoreLauncher/Program.cs`: el modo `--flujo-completo` delega el procedimiento a `RestoreAutomation`.
- `SkyPlus.RestoreLauncher/RestoreAutomation.cs` (**nuevo**): cierra la API, espera a que terminen API/Desktop/BackupWorker, crea el bloqueo de mantenimiento, invoca los tres comandos existentes de RestoreWorker, verifica `Completed`, retira mantenimiento y reinicia API/Desktop.
- `SkyPlus.Desktop/frmBackups.cs`: actualiza el mensaje de confirmación para describir el proceso automático.

No se cambió la lógica de importación de `RestoreWorker`, las autorizaciones RSA, el esquema MariaDB ni los archivos de configuración.

## Cómo probar en Windows/Visual Studio

1. Guardar una copia independiente de la base `skyplus` antes de probar una restauración de producción.
2. Comprobar que existen los archivos `.cnf`, claves y carpetas configurados en `restoreworker.json` y que la API puede iniciarse normalmente.
3. Abrir `SkyPlus.sln` en Visual Studio 2026 o con SDK .NET 10 y **recompilar la solución completa en Debug** (API, Desktop, RestoreWorker y RestoreLauncher).
4. Ejecutar API y Desktop como de costumbre. Verificar que `http://localhost:5004` está disponible y que se creó `%LOCALAPPDATA%\SkyPlus\api-shutdown.token`.
5. Desde Desktop seleccionar un backup **Disponible** y presionar **Restaurar producción**. Completar la segunda autorización administrativa.
6. El Desktop se cierra y se abre Launcher. Launcher solicita el apagado de la API, espera procesos, crea `C:\SkyPlus\Config\mantenimiento.lock`, genera y verifica un backup preventivo.
7. Antes de importar, Launcher solicita la confirmación literal `RESTAURAR SKYPLUS`. El Worker recibe también esa confirmación por stdin.
8. Tras la importación, Launcher comprueba el estado persistente `Completed` con verificaciones y desbloqueo de cuenta. Solo entonces elimina `mantenimiento.lock` y arranca API y Desktop.

## Advertencias y límites

- **No se probó una restauración real en este entorno**: aquí no hay Windows, MariaDB ni SDK `dotnet`. El código necesita compilarse y verificarse en la máquina del proyecto.
- La API reiniciada se ejecuta **fuera del depurador** con perfil `Development`, puertos HTTP 5004 / HTTPS 7198, y necesita que `dotnet` esté disponible en PATH y que el certificado HTTPS de desarrollo funcione.
- El Launcher presupone la estructura de carpetas de la solución Visual Studio; no está preparado para distribución en otra máquina sin la solución.
- Si hay un fallo, **no se reinician automáticamente las aplicaciones**. El bloqueo y el estado persistente pueden requerir revisión y recuperación manual. Nunca borres los archivos de estado para forzar el inicio.
- Si una autorización de 10 minutos vence durante el cierre o respaldo preventivo, la restauración se detiene sin importar datos.
- `SkyPlus.BackupWorker` es una tarea de ejecución puntual: el Launcher espera que termine, no la interrumpe durante un backup.
- No se ejecutan comandos de restauración automáticamente al abrir la solución: requieren autorización y confirmación explícita.
