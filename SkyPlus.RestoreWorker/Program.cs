using System.Runtime.Versioning;
using System.Text.Json;
using SkyPlus.RestoreWorker.Models;
using SkyPlus.RestoreWorker.Services;

[assembly: SupportedOSPlatform("windows")]

Console.WriteLine("=== SkyPlus RestoreWorker ===");

RestoreState? estado = null;
string? rutaEstado = null;

try
{
    // =========================================
    // 1. LEER CONFIGURACIÓN
    // =========================================

    string rutaConfig = Path.Combine(
        AppContext.BaseDirectory,
        "restoreworker.json");

    if (!File.Exists(rutaConfig))
        throw new InvalidOperationException(
            "No se encontró restoreworker.json.");

    using var documento = JsonDocument.Parse(
        await File.ReadAllTextAsync(rutaConfig));

    var config = documento.RootElement
        .GetProperty("Restore");

    string LeerConfig(string nombre) =>
        config.GetProperty(nombre).GetString()
        ?? throw new InvalidOperationException(
            $"Falta {nombre}.");

    string database = LeerConfig("Database");
    string testDatabase = LeerConfig("TestDatabase");
    string backupDirectory = LeerConfig("BackupDirectory");
    string auditDirectory = LeerConfig("AuditDirectory");
    string maintenanceFile = LeerConfig("MaintenanceFile");
    string mariaDbBinPath = LeerConfig("MariaDbBinPath");
    string credentialsFile = LeerConfig("BackupCredentialsFile");

    string preventiveDirectory =
        LeerConfig("PreventiveBackupDirectory");

    string monitorCredentialsFile =
        LeerConfig("MonitorCredentialsFile");

    string maintenanceCredentialsFile =
        LeerConfig("MaintenanceCredentialsFile");

    string restoreStateFile =
        LeerConfig("RestoreStateFile");

    string restoreLockFile =
        LeerConfig("RestoreLockFile");

    string authorizationPublicKeyFile =
        LeerConfig("AuthorizationPublicKeyFile");

    string accountAdminCredentialsFile =
    LeerConfig("AccountAdminCredentialsFile");

    string productionRestoreCredentialsFile =
        LeerConfig("ProductionRestoreCredentialsFile");

    rutaEstado = restoreStateFile;

    // =========================================
    // 2. VALIDAR CONFIGURACIÓN
    // =========================================

    if (!string.Equals(
        database,
        "skyplus",
        StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "La base principal configurada no es válida.");
    }

    if (string.Equals(
        database,
        testDatabase,
        StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "La base principal y la de pruebas deben ser diferentes.");
    }

    if (!Directory.Exists(backupDirectory))
        throw new DirectoryNotFoundException(
            "No existe el directorio de backups.");

    if (!Directory.Exists(auditDirectory))
        throw new DirectoryNotFoundException(
            "No existe el directorio de auditoría.");

    foreach (string archivo in new[]
{
        credentialsFile,
        monitorCredentialsFile,
        maintenanceCredentialsFile,
        authorizationPublicKeyFile,
        accountAdminCredentialsFile,
        productionRestoreCredentialsFile
})
    {
        if (!File.Exists(archivo))
        {
            throw new FileNotFoundException(
                "No existe un archivo requerido.",
                archivo);
        }
    }

    // =========================================
    // 3. BLOQUEO EXCLUSIVO
    // =========================================

    using var executionLock =
        RestoreExecutionLock.Adquirir(
            restoreLockFile);

    Console.WriteLine(
        "Bloqueo exclusivo de RestoreWorker: adquirido.");

    // =========================================
    // 3.1 VERIFICAR BLOQUEO DE APLICACIÓN
    // =========================================

    string applicationLockStateFile =
        Path.Combine(
            auditDirectory,
            "application-lock-state.json");

    if (await ApplicationLockRecoveryService
        .RequiereRecuperacionAsync(applicationLockStateFile))
    {
        throw new InvalidOperationException(
            "Existe un bloqueo de aplicación pendiente. " +
            "Se requiere recuperación administrativa " +
            "antes de continuar.");
    }

    Console.WriteLine(
        "Estado de bloqueo de aplicación: sin pendientes.");

    // =========================================
    // 3.1.0 VERIFICAR CONEXIÓN DE VALIDACIÓN
    // Uso: --verificar-conexion-validacion
    // Solo consulta identidad y base; no importa SQL.
    // =========================================

    if (args.Length > 0 && args[0] == "--verificar-conexion-validacion")
    {
        if (args.Length != 1)
            throw new InvalidOperationException(
                "Uso: RestoreWorker --verificar-conexion-validacion.");

        string credencialesValidacion = Path.Combine(
            Path.GetDirectoryName(credentialsFile)
                ?? throw new InvalidOperationException(
                    "Directorio de configuración inválido."),
            "validacion.cnf");

        await MariaDbRestoreService.VerificarConexionAsync(
            mariaDbBinPath,
            credencialesValidacion);

        Console.WriteLine("Verificación finalizada sin importar datos.");
        return;
    }

    // =========================================
    // 3.1.0.1 VERIFICAR INTEGRIDAD DE VALIDACIÓN
    // Uso: --verificar-integridad-validacion
    // Solo lectura sobre la base aislada.
    // =========================================

    if (args.Length > 0 && args[0] == "--verificar-integridad-validacion")
    {
        if (args.Length != 1)
            throw new InvalidOperationException(
                "Uso: RestoreWorker --verificar-integridad-validacion.");

        string credencialesValidacion = Path.Combine(
            Path.GetDirectoryName(credentialsFile)
                ?? throw new InvalidOperationException(
                    "Directorio de configuración inválido."),
            "validacion.cnf");

        await MariaDbRestoreService.VerificarIntegridadAsync(
            mariaDbBinPath,
            credencialesValidacion);

        Console.WriteLine("Validación finalizada sin modificar datos.");
        return;
    }

    // =========================================
    // 3.1.1 IMPORTACIÓN AISLADA DE VALIDACIÓN
    // Uso: --importar-validacion <ID_BACKUP>
    // No modifica estados ni credenciales de producción.
    // =========================================

    if (args.Length > 0 && args[0] == "--importar-validacion")
    {
        if (args.Length != 2 ||
            !Guid.TryParse(args[1], out Guid idValidacion) ||
            idValidacion == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Uso: RestoreWorker --importar-validacion <ID_BACKUP>.");
        }

        const string baseValidacion = "skyplus_restore_validacion";
        string credencialesValidacion = Path.Combine(
            Path.GetDirectoryName(credentialsFile)
                ?? throw new InvalidOperationException(
                    "Directorio de configuración inválido."),
            "validacion.cnf");

        if (!File.Exists(credencialesValidacion))
            throw new FileNotFoundException(
                "No se encontró validacion.cnf.",
                credencialesValidacion);

        // BackupVerifier solo verifica el respaldo registrado y su integridad.
        string backupValidado = await BackupVerifier.VerificarAsync(
            idValidacion,
            backupDirectory,
            auditDirectory);

        Console.WriteLine("=== IMPORTACIÓN DE VALIDACIÓN ===");
        Console.WriteLine("Destino exclusivo: " + baseValidacion);
        Console.WriteLine("Archivo: " + Path.GetFileName(backupValidado));
        Console.WriteLine(
            "ATENCIÓN: la base de validación puede quedar parcialmente " +
            "importada si ocurre un error.");
        Console.Write("Escribí CONFIRMAR para importar: ");

        if (Console.ReadLine() != "CONFIRMAR")
        {
            Console.WriteLine("Importación cancelada.");
            return;
        }

        await MariaDbRestoreService.ImportarValidacionAsync(
            mariaDbBinPath,
            credencialesValidacion,
            backupValidado,
            baseValidacion);

        Console.WriteLine("Importación finalizada; falta verificar datos.");
        return;
    }

    // =========================================
    // 3.2 VERIFICAR RESTAURACIÓN DE PRODUCCIÓN
    // =========================================

    string productionRestoreStateFile =
        Path.Combine(
            auditDirectory,
            "production-restore-state.json");

    bool verificarContinuacionSolicitada =
        args.Length == 3 &&
        (args[0] == "--verificar-continuacion" ||
         args[0] == "--continuar-produccion");

    if (!verificarContinuacionSolicitada)
    {
        await ProductionRestoreStateService
            .VerificarSinPendientesAsync(
                productionRestoreStateFile);

        Console.WriteLine(
            "Estado de restauración de producción: sin pendientes.");
    }
    else
    {
        Console.WriteLine(
            "Modo de verificación de operación pendiente.");
    }

    // =========================================
    // 4. VERIFICAR ESTADO PERSISTENTE
    // =========================================

    RestoreState? estadoAnterior =
        await RestoreStateService.LeerAsync(
            restoreStateFile);

    if (estadoAnterior is not null)
    {
        Console.WriteLine(
            $"Última operación: " +
            $"{estadoAnterior.OperationType} - " +
            $"{estadoAnterior.Phase}");
    }

    await RestoreStateService
        .VerificarOperacionPendienteAsync(
            restoreStateFile);

    Console.WriteLine(
        "Estado de restauración: sin operaciones pendientes.");

    // =========================================
    // 5. VALIDAR MODO MANTENIMIENTO
    // =========================================

    if (!File.Exists(maintenanceFile))
    {
        throw new InvalidOperationException(
            "SkyPlus no está en modo mantenimiento.");
    }

    // =========================================
    // 6. INTERPRETAR ARGUMENTOS
    // =========================================

    bool pruebaReadOnly =
        args.Length == 2 &&
        args[0] == "--test-read-only";

    bool pruebaAppLock =
        args.Length == 1 &&
        args[0] == "--test-app-lock";

    bool verificacionesPreliminares =
        args.Length == 1 &&
        !pruebaAppLock;

    bool autorizacionProduccion =
        args.Length == 3 &&
        args[0] == "--autorizar-produccion";

    bool prepararProduccion =
        args.Length == 3 &&
        args[0] == "--preparar-produccion";

    bool verificarContinuacion =
        args.Length == 3 &&
        args[0] == "--verificar-continuacion";

    bool continuarProduccion =
        args.Length == 3 &&
        args[0] == "--continuar-produccion";

    bool backupPreventivoProduccion =
        args.Length == 3 &&
        args[0] == "--backup-preventivo";

    if (!pruebaReadOnly &&
        !pruebaAppLock &&
        !verificacionesPreliminares &&
        !autorizacionProduccion &&
        !verificarContinuacion &&
        !continuarProduccion &&
        !prepararProduccion &&
        !backupPreventivoProduccion)
    {
        throw new InvalidOperationException(
            "Uso: RestoreWorker <ID_BACKUP>, " +
            "RestoreWorker --test-read-only <ID_BACKUP>, " +
            "RestoreWorker --test-app-lock, " +
            "RestoreWorker --autorizar-produccion <ID_BACKUP> <ARCHIVO>, " +
            "RestoreWorker --preparar-produccion <ID_BACKUP> <ARCHIVO>, " +
            "RestoreWorker --backup-preventivo <ID_BACKUP> <ARCHIVO>");
    }

    Guid idBackup = Guid.Empty;

    if (!pruebaAppLock)
    {
        string argumentoId = pruebaReadOnly
            ? args[1]
            : verificacionesPreliminares
                ? args[0]
                : args[1];

        if (!Guid.TryParse(
            argumentoId,
            out idBackup) ||
            idBackup == Guid.Empty)
        {
            throw new InvalidOperationException(
                "El identificador del backup no es válido.");
        }
    }

    // =========================================
    // 7. VERIFICAR PROCESOS SKYPLUS
    // =========================================

    IsolationVerifier.VerificarProcesos();

    Console.WriteLine(
        "Procesos de SkyPlus: detenidos.");

    // =========================================
    // 8. VERIFICAR READ_ONLY
    // =========================================

    bool soloLectura =
        await MariaDbReadOnlyService.ObtenerEstadoAsync(
            maintenanceCredentialsFile);

    Console.WriteLine(
        "MariaDB read_only: " +
        (soloLectura ? "ON" : "OFF"));

    if (soloLectura)
    {
        throw new InvalidOperationException(
            "MariaDB ya está en modo solo lectura. " +
            "Se requiere revisión administrativa.");
    }

    // =========================================
    // 9. VERIFICAR CONEXIONES MARIADB
    // =========================================

    await MariaDbIsolationVerifier.VerificarAsync(
        monitorCredentialsFile,
        database);

    Console.WriteLine(
        "Conexiones a skyplus: verificadas.");

    // =========================================
    // VERIFICAR CONTINUACIÓN DE PRODUCCIÓN
    // =========================================

    if (verificarContinuacion)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=== VERIFICACIÓN DE CONTINUACIÓN ===");

        string rutaAutorizacion = args[2];

        if (!File.Exists(rutaAutorizacion))
            throw new FileNotFoundException(
                "No existe el archivo de autorización.",
                rutaAutorizacion);

        string documentoFirmado =
            await File.ReadAllTextAsync(rutaAutorizacion);

        var estadoPendiente =
            await ProductionRestoreStateService.LeerAsync(
                productionRestoreStateFile);

        if (estadoPendiente is null)
        {
            throw new InvalidOperationException(
                "No existe una restauración de producción pendiente.");
        }

        if (estadoPendiente.OperationId == Guid.Empty ||
            estadoPendiente.BackupId != idBackup)
        {
            throw new InvalidOperationException(
                "Los identificadores de la operación pendiente no son válidos.");
        }

        // Validar firma RSA, propósito, vigencia y operación.
        // El OperationId esperado se obtiene del estado registrado,
        // no del documento de autorización.
        RestoreAuthorizationValidationService.ValidarContinuacion(
        documentoFirmado,
        authorizationPublicKeyFile,
        idBackup,
        estadoPendiente.OperationId);

        // Verificar fase e integridad del backup preventivo.
        await ProductionRestoreStateService.ValidarContinuacionAsync(
            productionRestoreStateFile,
            estadoPendiente.OperationId,
            idBackup);

        Console.WriteLine(
            "Continuación verificada correctamente.");

        Console.WriteLine(
            "No se modificó la base de datos.");

        Console.WriteLine(
            "El estado pendiente permanece registrado.");

        return;
    }

    // =========================================
    // 9.0 CONTINUAR RESTAURACIÓN DE PRODUCCIÓN
    // Uso: --continuar-produccion <ID_BACKUP> <ARCHIVO_AUTORIZACION>
    // DESTRUCTIVO: conserva el bloqueo exclusivo hasta terminar.
    // =========================================

    if (continuarProduccion)
    {
        Console.WriteLine();
        Console.WriteLine("=== CONTINUACIÓN DE PRODUCCIÓN ===");

        string rutaAutorizacion = args[2];
        if (!File.Exists(rutaAutorizacion))
            throw new FileNotFoundException(
                "No existe el archivo de autorización.", rutaAutorizacion);

        string documentoFirmado =
            await File.ReadAllTextAsync(rutaAutorizacion);

        var estadoPendiente =
            await ProductionRestoreStateService.LeerAsync(
                productionRestoreStateFile)
            ?? throw new InvalidOperationException(
                "No existe una restauración de producción pendiente.");

        if (estadoPendiente.OperationId == Guid.Empty ||
            estadoPendiente.BackupId != idBackup)
            throw new InvalidOperationException(
                "La operación pendiente no coincide con el backup solicitado.");

        // Verificar sin modificar producción ni consumir la autorización.
        RestoreAuthorizationValidationService.ValidarContinuacion(
            documentoFirmado,
            authorizationPublicKeyFile,
            idBackup,
            estadoPendiente.OperationId);

        await ProductionRestoreStateService.ValidarContinuacionAsync(
            productionRestoreStateFile,
            estadoPendiente.OperationId,
            idBackup);

        await BackupVerifier.VerificarAsync(
            idBackup, backupDirectory, auditDirectory);

        await MariaDbProductionRestoreService.VerificarConexionAsync(
            mariaDbBinPath, productionRestoreCredentialsFile);

        Console.WriteLine("Operación: " + estadoPendiente.OperationId);
        Console.WriteLine("Backup: " + idBackup);
        Console.WriteLine("Se reemplazarán datos de la base skyplus.");
        Console.WriteLine("Si falla la operación, puede requerir recuperación manual.");
        Console.WriteLine("La aplicación puede permanecer bloqueada.");
        Console.WriteLine();
        Console.Write("Escribí RESTAURAR SKYPLUS para confirmar: ");

        if (Console.ReadLine() != "RESTAURAR SKYPLUS")
        {
            Console.WriteLine("Operación cancelada. No se modificó producción.");
            return;
        }

        // La firma se valida nuevamente en el orquestador antes del bloqueo.
        await ProductionRestoreOrchestrator.ContinuarProduccionAsync(
            estadoPendiente.OperationId,
            idBackup,
            documentoFirmado,
            authorizationPublicKeyFile,
            backupDirectory,
            auditDirectory,
            productionRestoreStateFile,
            maintenanceFile,
            mariaDbBinPath,
            productionRestoreCredentialsFile,
            accountAdminCredentialsFile,
            monitorCredentialsFile,
            ct => MariaDbProductionVerificationService.VerificarAsync(
                mariaDbBinPath, productionRestoreCredentialsFile, ct));

        return;
    }

    // =========================================
    // 9.1 PRUEBA CONTROLADA DE ACCOUNT LOCK
    // =========================================

    if (pruebaAppLock)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=== PRUEBA DE BLOQUEO DE APLICACIÓN ===");

        // No desbloquear una cuenta bloqueada previamente
        // por una operación distinta a esta prueba.
        if (await MariaDbApplicationLockService.EstaBloqueadaAsync(
            mariaDbBinPath,
            accountAdminCredentialsFile))
        {
            throw new InvalidOperationException(
                "skyplus_app ya está bloqueada. " +
                "Se requiere revisión administrativa.");
        }

        Console.WriteLine(
            "Estado inicial: skyplus_app desbloqueada.");
        Console.WriteLine(
            "Se bloqueará temporalmente skyplus_app@localhost.");
        Console.WriteLine(
            "No se modificarán datos de skyplus.");
        Console.WriteLine();
        Console.Write(
            "Escribí CONFIRMAR para continuar: ");

        string? confirmacion = Console.ReadLine();

        if (confirmacion != "CONFIRMAR")
        {
            Console.WriteLine(
                "Prueba cancelada. No se modificó MariaDB.");
            return;
        }

        await ApplicationLockTestService.EjecutarAsync(
            mariaDbBinPath,
            accountAdminCredentialsFile,
            applicationLockStateFile);

        return;
    }

    // =========================================
    // 9.2 PREPARAR RESTAURACIÓN DE PRODUCCIÓN
    // =========================================

    if (prepararProduccion)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=== PREPARACIÓN DE RESTAURACIÓN ===");

        string rutaAutorizacion = args[2];

        if (!File.Exists(rutaAutorizacion))
        {
            throw new FileNotFoundException(
                "No existe el archivo de autorización.",
                rutaAutorizacion);
        }

        string documentoFirmado =
            await File.ReadAllTextAsync(rutaAutorizacion);

        var autorizacion =
            RestoreAuthorizationValidationService.Validar(
                documentoFirmado,
                authorizationPublicKeyFile,
                idBackup);

        await ProductionRestoreOrchestrator.PrepararAsync(
            autorizacion.OperationId,
            idBackup,
            backupDirectory,
            auditDirectory,
            productionRestoreStateFile,
            documentoFirmado,
            authorizationPublicKeyFile);

        Console.WriteLine(
            "Preparación de producción finalizada.");

        Console.WriteLine(
            "No se consumió la autorización.");

        Console.WriteLine(
            "No se realizó ninguna restauración.");

        return;
    }

    // =========================================
    // 9.3 BACKUP PREVENTIVO DE PRODUCCIÓN
    // =========================================

    if (backupPreventivoProduccion)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=== BACKUP PREVENTIVO DE PRODUCCIÓN ===");

        string rutaAutorizacion = args[2];

        if (!File.Exists(rutaAutorizacion))
            throw new FileNotFoundException(
                "No existe el archivo de autorización.",
                rutaAutorizacion);

        string documentoFirmado =
            await File.ReadAllTextAsync(rutaAutorizacion);

        // Validar la firma y la vigencia ANTES de iniciar
        // cualquier registro persistente.
        var autorizacion =
            RestoreAuthorizationValidationService.Validar(
                documentoFirmado,
                authorizationPublicKeyFile,
                idBackup);

        // Comprobar que el backup que se desea restaurar
        // existe y coincide con su hash registrado.
        await ProductionRestoreOrchestrator.PrepararAsync(
            autorizacion.OperationId,
            idBackup,
            backupDirectory,
            auditDirectory,
            productionRestoreStateFile,
            documentoFirmado,
            authorizationPublicKeyFile);

        Console.WriteLine(
            "Se generará una copia preventiva de skyplus.");
        Console.WriteLine(
            "La operación quedará pendiente de continuación o recuperación.");
        Console.WriteLine(
            "No se importará SQL ni se consumirá la autorización.");
        Console.WriteLine();
        Console.Write("Escribí CONFIRMAR para continuar: ");

        if (Console.ReadLine() != "CONFIRMAR")
        {
            Console.WriteLine("Operación cancelada.");
            return;
        }

        string rutaPreventiva =
            await ProductionRestoreOrchestrator.GenerarBackupPreventivoAsync(
                autorizacion.OperationId,
                idBackup,
                mariaDbBinPath,
                credentialsFile,
                preventiveDirectory,
                productionRestoreStateFile);

        Console.WriteLine("Archivo: " + rutaPreventiva);
        Console.WriteLine(
            "Estado: PreventiveBackupVerified (pendiente).");
        Console.WriteLine(
            "No se modificaron tablas ni se realizó restauración.");
        return;
    }

    // =========================================
    // 10. VALIDAR AUTORIZACIÓN DE PRODUCCIÓN
    // =========================================

    if (autorizacionProduccion)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=== VALIDACIÓN DE AUTORIZACIÓN DE PRODUCCIÓN ===");

        string rutaAutorizacion = args[2];

        if (!File.Exists(rutaAutorizacion))
        {
            throw new FileNotFoundException(
                "No existe el archivo de autorización.",
                rutaAutorizacion);
        }

        string documentoFirmado =
            await File.ReadAllTextAsync(
                rutaAutorizacion);

        var autorizacion =
            RestoreAuthorizationValidationService.Validar(
                documentoFirmado,
                authorizationPublicKeyFile,
                idBackup);

        Console.WriteLine(
            "Autorización firmada verificada correctamente.");

        Console.WriteLine(
            $"Backup autorizado: {autorizacion.BackupId}");

        Console.WriteLine(
            $"Operación autorizada: {autorizacion.OperationId}");

        Console.WriteLine(
            $"Administrador solicitante: " +
            $"{autorizacion.FirstAdministratorId}");

        Console.WriteLine(
            $"Administrador autorizante: " +
            $"{autorizacion.SecondAdministratorId}");

        Console.WriteLine(
            $"Vencimiento UTC: " +
            $"{autorizacion.ExpiresAtUtc:O}");

        Console.WriteLine();
        Console.WriteLine(
            "Validación finalizada correctamente.");

        Console.WriteLine(
            "No se consumió la autorización.");

        Console.WriteLine(
            "No se realizó ninguna restauración.");

        return;
    }

    // =========================================
    // 11. PRUEBA CONTROLADA READ_ONLY
    // =========================================

    if (pruebaReadOnly)
    {
        Console.WriteLine();
        Console.WriteLine(
            "=== PRUEBA CONTROLADA READ_ONLY ===");

        Console.WriteLine(
            "ATENCIÓN: esta operación modificará " +
            "temporalmente la configuración GLOBAL " +
            "de MariaDB.");

        Console.WriteLine(
            "Puede afectar a todas las bases del servidor.");

        Console.WriteLine(
            "No se restaurarán datos.");

        Console.WriteLine();
        Console.Write(
            "Escribí CONFIRMAR para continuar: ");

        string? confirmacion =
            Console.ReadLine();

        if (confirmacion != "CONFIRMAR")
        {
            Console.WriteLine(
                "Operación cancelada. " +
                "No se modificó MariaDB.");

            return;
        }

        Console.WriteLine(
            "Confirmación recibida.");

        await ReadOnlyTestService.EjecutarAsync(
            maintenanceCredentialsFile,
            restoreStateFile,
            idBackup);

        Console.WriteLine(
            "Prueba controlada finalizada.");

        return;
    }

    // =========================================
    // 12. INICIAR REGISTRO DE OPERACIÓN
    // =========================================

    estado = new RestoreState
    {
        OperationId = Guid.NewGuid(),
        BackupId = idBackup,
        OperationType = "PreliminaryCheck",
        Phase = "Created",
        StartedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow,
        PreliminaryChecksCompleted = false,
        ReadOnlyActivated = false,
        RestorationStarted = false,
        RestorationCompleted = false
    };

    await RestoreStateService.GuardarAsync(
        restoreStateFile,
        estado);

    Console.WriteLine(
        $"Operación registrada: {estado.OperationId}");

    // =========================================
    // 13. VERIFICAR BACKUP SELECCIONADO
    // =========================================

    string rutaBackup =
        await BackupVerifier.VerificarAsync(
            idBackup,
            backupDirectory,
            auditDirectory);

    await ProductionRestoreService.PrepararAsync(
        rutaBackup,
        database);

    await RestoreStateService.ActualizarFaseAsync(
        restoreStateFile,
        estado,
        "BackupVerified");

    Console.WriteLine(
        "Backup seleccionado: " + idBackup);

    Console.WriteLine(
        "Integridad SHA-256: correcta.");

    Console.WriteLine(
        "Archivo verificado: " +
        Path.GetFileName(rutaBackup));

    // =========================================
    // 14. GENERAR BACKUP PREVENTIVO
    // =========================================

    string backupPreventivo =
        await PreventiveBackupService.GenerarAsync(
            mariaDbBinPath,
            credentialsFile,
            database,
            preventiveDirectory);

    await RestoreStateService.ActualizarFaseAsync(
        restoreStateFile,
        estado,
        "PreventiveBackupCreated");

    Console.WriteLine(
        "Backup preventivo generado correctamente.");

    Console.WriteLine(
        "Archivo: " +
        Path.GetFileName(backupPreventivo));

    Console.WriteLine(
        "SHA-256 registrado correctamente.");

    // =========================================
    // 15. FINALIZAR VERIFICACIONES
    // =========================================

    estado.PreliminaryChecksCompleted = true;

    await RestoreStateService.ActualizarFaseAsync(
        restoreStateFile,
        estado,
        "PreliminaryChecksCompleted");

    Console.WriteLine();
    Console.WriteLine(
        "Configuración: correcta.");

    Console.WriteLine(
        "Modo mantenimiento: activado.");

    Console.WriteLine(
        "Directorios: disponibles.");

    Console.WriteLine(
        "Base principal: " + database);

    Console.WriteLine(
        "Base de pruebas: " + testDatabase);

    Console.WriteLine();
    Console.WriteLine(
        "Verificaciones preliminares completadas.");

    Console.WriteLine(
        "No se realizó ninguna restauración.");
}
catch (Exception ex)
{
    Console.Error.WriteLine(
        "ERROR: " + ex.Message);

    Environment.ExitCode = 1;

    if (estado is not null &&
        !string.IsNullOrWhiteSpace(rutaEstado))
    {
        try
        {
            estado.Error = ex.Message;

            await RestoreStateService.ActualizarFaseAsync(
                rutaEstado,
                estado,
                "Failed");
        }
        catch (Exception errorRegistro)
        {
            Console.Error.WriteLine(
                "No se pudo registrar el error: " +
                errorRegistro.Message);
        }
    }
}