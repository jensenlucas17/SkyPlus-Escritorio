using System.Security.Cryptography;
using System.Text.Json;
using SkyPlus.Domain.Backup;
using SkyPlus.RestoreWorker.Services;

Console.WriteLine("=== SKYPLUS: PRUEBAS DE SEGURIDAD 7 A 45 ===");

string directorioTemporal = Path.Combine(Path.GetTempPath(), $"skyplus-security-tests-{Guid.NewGuid():N}");
Directory.CreateDirectory(directorioTemporal);
string rutaClave = Path.Combine(directorioTemporal, "test-public.pem");
string rutaEstado = Path.Combine(directorioTemporal, "production-restore-state.json");
string rutaBackup = Path.Combine(directorioTemporal, "preventivo-test.sql");
int aprobadas = 0;
int fallidas = 0;

async Task ProbarAsync(int numero, string titulo, Func<Task> accion, Type tipo, string mensaje)
{
    Console.WriteLine($"\n=== PRUEBA {numero}: {titulo} ===");
    try
    {
        await accion();
        Console.WriteLine($"PRUEBA {numero} FALLIDA: no se produjo el rechazo esperado.");
        fallidas++;
    }
    catch (Exception ex) when (ex.GetType() == tipo && ex.Message == mensaje)
    {
        Console.WriteLine($"PRUEBA {numero} APROBADA: {ex.Message}");
        aprobadas++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"PRUEBA {numero} FALLIDA: {ex.GetType().Name}: {ex.Message}");
        fallidas++;
    }
}

async Task ProbarExitoAsync(int numero, string titulo, Func<Task> accion)
{
    Console.WriteLine($"\n=== PRUEBA {numero}: {titulo} ===");
    try
    {
        await accion();
        Console.WriteLine($"PRUEBA {numero} APROBADA: transición válida persistida y verificada.");
        aprobadas++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"PRUEBA {numero} FALLIDA: {ex.GetType().Name}: {ex.Message}");
        fallidas++;
    }
}

try
{
    using RSA rsa = RSA.Create(2048);
    File.WriteAllText(rutaClave, rsa.ExportSubjectPublicKeyInfoPem());
    Guid backupId = Guid.NewGuid();
    Guid operationId = Guid.NewGuid();
    DateTime ahora = DateTime.UtcNow;

    string Firmar(RestoreAuthorizationPayload p)
    {
        byte[] datos = JsonSerializer.SerializeToUtf8Bytes(p);
        byte[] firma = rsa.SignData(datos, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        return JsonSerializer.Serialize(new { Payload = Convert.ToBase64String(datos), Signature = Convert.ToBase64String(firma) });
    }

    var vencido = new RestoreAuthorizationPayload
    {
        OperationId = operationId,
        BackupId = backupId,
        FirstAdministratorId = 1,
        SecondAdministratorId = 2,
        ApprovedAtUtc = ahora.AddMinutes(-15),
        ExpiresAtUtc = ahora.AddMinutes(-5),
        Purpose = "ProductionRestore",
        TargetDatabase = "skyplus"
    };
    string docVencido = Firmar(vencido);
    await ProbarAsync(7, "AUTORIZACIÓN VENCIDA", () => { RestoreAuthorizationValidationService.Validar(docVencido, rutaClave, backupId); return Task.CompletedTask; }, typeof(InvalidOperationException), "La autorización está vencida.");

    byte[] datosVencidos = JsonSerializer.SerializeToUtf8Bytes(vencido);
    byte[] firmaAlterada = rsa.SignData(datosVencidos, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
    firmaAlterada[0] ^= 0xFF;
    string docAlterado = JsonSerializer.Serialize(new { Payload = Convert.ToBase64String(datosVencidos), Signature = Convert.ToBase64String(firmaAlterada) });
    await ProbarAsync(8, "FIRMA RSA ALTERADA", () => { RestoreAuthorizationValidationService.Validar(docAlterado, rutaClave, backupId); return Task.CompletedTask; }, typeof(InvalidOperationException), "La firma digital de restauración no es válida.");

    var vigente = new RestoreAuthorizationPayload
    {
        OperationId = operationId,
        BackupId = backupId,
        FirstAdministratorId = 1,
        SecondAdministratorId = 2,
        ApprovedAtUtc = ahora.AddMinutes(-1),
        ExpiresAtUtc = ahora.AddMinutes(8),
        Purpose = "ProductionRestore",
        TargetDatabase = "skyplus"
    };
    string docVigente = Firmar(vigente);
    await ProbarAsync(9, "BACKUP NO AUTORIZADO", () => { RestoreAuthorizationValidationService.Validar(docVigente, rutaClave, Guid.NewGuid()); return Task.CompletedTask; }, typeof(InvalidOperationException), "La autorización no corresponde al backup.");
    await ProbarAsync(10, "PROPÓSITO INCORRECTO", () => { RestoreAuthorizationValidationService.ValidarContinuacion(docVigente, rutaClave, backupId, operationId); return Task.CompletedTask; }, typeof(InvalidOperationException), "El propósito de la autorización no corresponde a la operación solicitada.");

    var continuacion = new RestoreAuthorizationPayload
    {
        OperationId = operationId,
        BackupId = backupId,
        FirstAdministratorId = 1,
        SecondAdministratorId = 2,
        ApprovedAtUtc = ahora.AddMinutes(-1),
        ExpiresAtUtc = ahora.AddMinutes(8),
        Purpose = "ProductionRestoreContinuation",
        TargetDatabase = "skyplus"
    };
    string docContinuacion = Firmar(continuacion);
    await ProbarAsync(11, "OPERATION ID INCORRECTO", () => { RestoreAuthorizationValidationService.ValidarContinuacion(docContinuacion, rutaClave, backupId, Guid.NewGuid()); return Task.CompletedTask; }, typeof(InvalidOperationException), "La autorización no corresponde a la operación pendiente.");

    await ProbarAsync(12, "OPERACIÓN PENDIENTE INEXISTENTE", async () => { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }, typeof(InvalidOperationException), "No existe una operación de producción registrada.");

    var estado = new ProductionRestoreStateService.ProductionRestoreState
    {
        OperationId = operationId,
        BackupId = backupId,
        Phase = "PreventiveBackupVerified",
        PreventiveBackupPath = rutaBackup,
        PreventiveBackupSha256 = new string('a', 64),
        UpdatedAtUtc = ahora
    };
    async Task GuardarEstado() => await File.WriteAllTextAsync(rutaEstado, JsonSerializer.Serialize(estado));
    await GuardarEstado();
    await ProbarAsync(13, "OPERATION ID DEL ESTADO INCORRECTO", async () => { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, Guid.NewGuid(), backupId); }, typeof(InvalidOperationException), "El ID de operación no coincide.");
    await ProbarAsync(14, "BACKUP ID DEL ESTADO INCORRECTO", async () => { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, Guid.NewGuid()); }, typeof(InvalidOperationException), "El ID del backup no coincide.");
    estado.Phase = "ImportStarted";
    await GuardarEstado();
    await ProbarAsync(15, "FASE INCORRECTA", async () => { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }, typeof(InvalidOperationException), "La operación no está en la fase esperada. Fase actual: ImportStarted");

    estado.Phase = "PreventiveBackupVerified";
    await GuardarEstado();
    await ProbarAsync(16, "BACKUP PREVENTIVO INEXISTENTE", async () => { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }, typeof(FileNotFoundException), "No existe el backup preventivo registrado.");

    estado.PreventiveBackupSha256 = "";
    await GuardarEstado();
    await ProbarAsync(17, "SHA-256 PREVENTIVO AUSENTE", async () => { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }, typeof(InvalidOperationException), "No se registró el SHA-256 preventivo.");

    await File.WriteAllTextAsync(rutaBackup, "Archivo temporal para verificar SHA-256; no es un respaldo real.");
    string hashOriginal = await PreventiveBackupVerificationService.VerificarAsync(rutaBackup);
    estado.PreventiveBackupSha256 = hashOriginal;
    await File.WriteAllTextAsync(rutaBackup + ".sha256", hashOriginal);
    await GuardarEstado();

    await File.AppendAllTextAsync(rutaBackup, "\nArchivo modificado para la prueba.");
    await ProbarAsync(18, "BACKUP PREVENTIVO MODIFICADO", async () => { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }, typeof(InvalidOperationException), "El backup preventivo fue modificado después de su verificación.");

    await File.WriteAllTextAsync(rutaBackup, "Archivo temporal para verificar SHA-256; no es un respaldo real.");
    string hashRestaurado = await PreventiveBackupVerificationService.VerificarAsync(rutaBackup);
    estado.PreventiveBackupSha256 = hashRestaurado;
    await GuardarEstado();
    File.Delete(rutaBackup + ".sha256");
    await ProbarAsync(19, "ARCHIVO .SHA256 INEXISTENTE", async () => { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }, typeof(FileNotFoundException), "No existe el archivo SHA-256 preventivo.");

    await File.WriteAllTextAsync(rutaBackup + ".sha256", new string('0', 64));
    await ProbarAsync(20, "ARCHIVO .SHA256 INCORRECTO", async () => { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }, typeof(InvalidOperationException), "El archivo SHA-256 preventivo no coincide.");

    // PRUEBAS 21 A 25: solamente estados y archivos temporales.
    await File.WriteAllTextAsync(rutaBackup + ".sha256", hashRestaurado);

    ProductionRestoreStateService.ProductionRestoreState EstadoPrueba(
        string fase, bool bloqueo = false, bool importacion = false,
        bool importacionCompletada = false, bool verificacion = false, bool desbloqueo = false,
        string? error = null) => new()
        {
            OperationId = operationId,
            BackupId = backupId,
            Phase = fase,
            PreventiveBackupPath = rutaBackup,
            PreventiveBackupSha256 = hashRestaurado,
            ApplicationLockAttempted = bloqueo,
            ImportStarted = importacion,
            ImportCompleted = importacionCompletada,
            VerificationCompleted = verificacion,
            ApplicationUnlocked = desbloqueo,
            Error = error,
            UpdatedAtUtc = DateTime.UtcNow
        };

    async Task SembrarEstadoAsync(ProductionRestoreStateService.ProductionRestoreState nuevo) =>
        await File.WriteAllTextAsync(rutaEstado, JsonSerializer.Serialize(nuevo));

    await SembrarEstadoAsync(EstadoPrueba("PreventiveBackupVerified"));
    await ProbarAsync(21, "RECHAZAR NUEVA OPERACIÓN CON OTRA PENDIENTE", async () =>
    {
        await ProductionRestoreStateService.CrearAsync(rutaEstado, new()
        {
            OperationId = Guid.NewGuid(),
            BackupId = Guid.NewGuid(),
            Phase = "PreventiveBackupStarted"
        });
    }, typeof(InvalidOperationException), "Existe una restauración pendiente.");

    await SembrarEstadoAsync(EstadoPrueba("ImportStarted", bloqueo: true, importacion: true));
    await ProbarAsync(22, "IMPEDIR RETROCESO DESDE IMPORTSTARTED", async () =>
    {
        await ProductionRestoreStateService.ActualizarAsync(
            rutaEstado, EstadoPrueba("ApplicationIsolated", bloqueo: true));
    }, typeof(InvalidOperationException), "No se pueden revertir indicadores de ejecución.");

    await SembrarEstadoAsync(EstadoPrueba("PreventiveBackupVerified"));
    await ProbarAsync(23, "RECHAZAR SALTO DE FASE OBLIGATORIA", async () =>
    {
        await ProductionRestoreStateService.ActualizarAsync(
            rutaEstado, EstadoPrueba("ImportStarted", bloqueo: true, importacion: true));
    }, typeof(InvalidOperationException),
       "Transición de fase no permitida: PreventiveBackupVerified → ImportStarted");

    await SembrarEstadoAsync(EstadoPrueba("ImportStarted", bloqueo: true, importacion: true));
    await ProbarExitoAsync(24, "REGISTRAR REQUIRESRECOVERY SIN PERDER INDICADORES", async () =>
    {
        await ProductionRestoreStateService.ActualizarAsync(
            rutaEstado, EstadoPrueba("RequiresRecovery", bloqueo: true,
                importacion: true, error: "Fallo simulado; requiere revisión administrativa."));
        var persistido = await ProductionRestoreStateService.LeerAsync(rutaEstado)
            ?? throw new InvalidOperationException("El estado no fue persistido.");
        if (persistido.Phase != "RequiresRecovery" || !persistido.ApplicationLockAttempted ||
            !persistido.ImportStarted || persistido.ImportCompleted ||
            persistido.VerificationCompleted || persistido.ApplicationUnlocked ||
            string.IsNullOrWhiteSpace(persistido.Error))
            throw new InvalidOperationException("Los indicadores de recuperación no se conservaron.");
    });

    await SembrarEstadoAsync(EstadoPrueba("ImportStarted", bloqueo: true, importacion: true));
    await ProbarAsync(25, "RECHAZAR CONTINUACIÓN TRAS INICIAR IMPORTACIÓN", async () =>
    {
        await ProductionRestoreStateService.ValidarContinuacionAsync(
            rutaEstado, operationId, backupId);
    }, typeof(InvalidOperationException),
       "La operación no está en la fase esperada. Fase actual: ImportStarted");

    // PRUEBAS 26-30: marcador de importación. Nunca se inicia MariaDB.
    Guid intentoOp = Guid.NewGuid();
    Guid intentoBackup = Guid.NewGuid();
    string rutaMarcador = ProductionImportAttemptService.ObtenerRuta(rutaEstado, intentoOp);

    await ProbarExitoAsync(26, "REGISTRAR INTENTO ÚNICO", async () =>
    {
        await ProductionImportAttemptService.RegistrarAsync(rutaEstado, intentoOp, intentoBackup);
        if (!File.Exists(rutaMarcador)) throw new InvalidOperationException("No se creó el marcador.");
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(rutaMarcador));
        if (doc.RootElement.GetProperty("OperationId").GetGuid() != intentoOp ||
            doc.RootElement.GetProperty("BackupId").GetGuid() != intentoBackup)
            throw new InvalidOperationException("El marcador no contiene los identificadores correctos.");
    });

    await ProbarAsync(27, "RECHAZAR SEGUNDO INTENTO", async () =>
    {
        await ProductionImportAttemptService.RegistrarAsync(rutaEstado, intentoOp, intentoBackup);
    }, typeof(InvalidOperationException),
       "Ya existe un intento de importación registrado para esta operación.");

    await ProbarExitoAsync(28, "CONSERVAR MARCADOR TRAS REINICIO SIMULADO", async () =>
    {
        // La instancia anterior ya terminó; el archivo sigue presente.
        if (!File.Exists(rutaMarcador)) throw new InvalidOperationException("Se perdió el marcador persistente.");
        string contenido = await File.ReadAllTextAsync(rutaMarcador);
        if (!contenido.Contains(intentoOp.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Se perdió la identidad del intento.");
    });

    await ProbarExitoAsync(29, "PERMITIR MARCADOR DE OTRA OPERACIÓN", async () =>
    {
        Guid otraOp = Guid.NewGuid();
        await ProductionImportAttemptService.RegistrarAsync(rutaEstado, otraOp, intentoBackup);
        if (!File.Exists(ProductionImportAttemptService.ObtenerRuta(rutaEstado, otraOp)))
            throw new InvalidOperationException("No se creó el segundo marcador.");
    });

    await ProbarExitoAsync(30, "CONCURRENCIA: SOLO UN INTENTO", async () =>
    {
        Guid concurrente = Guid.NewGuid();
        Task<bool>[] tareas = Enumerable.Range(0, 8).Select(_ => Task.Run(async () =>
        {
            try
            {
                await ProductionImportAttemptService.RegistrarAsync(rutaEstado, concurrente, intentoBackup);
                return true;
            }
            catch (InvalidOperationException ex) when (
                ex.Message == "Ya existe un intento de importación registrado para esta operación.")
            {
                return false;
            }
        })).ToArray();
        bool[] resultados = await Task.WhenAll(tareas);
        if (resultados.Count(x => x) != 1)
            throw new InvalidOperationException("La concurrencia permitió más de un intento.");
    });


    // PRUEBAS 31-40: fallos simulados; no se ejecuta MariaDB.
    await ProbarAsync(31, "RECHAZAR OPERATIONID VACÍO", async () =>
    {
        await ProductionImportAttemptService.RegistrarAsync(rutaEstado, Guid.Empty, backupId);
    }, typeof(InvalidOperationException), "OperationId inválido.");

    await ProbarAsync(32, "RECHAZAR BACKUPID VACÍO", async () =>
    {
        await ProductionImportAttemptService.RegistrarAsync(rutaEstado, Guid.NewGuid(), Guid.Empty);
    }, typeof(InvalidOperationException), "BackupId inválido.");

    await ProbarAsync(33, "RECHAZAR DIRECTORIO DE AUDITORÍA INEXISTENTE", async () =>
    {
        string inexistente = Path.Combine(directorioTemporal, "no-existe", "estado.json");
        await ProductionImportAttemptService.RegistrarAsync(inexistente, Guid.NewGuid(), backupId);
    }, typeof(DirectoryNotFoundException), "No existe el directorio de auditoría protegido.");

    await ProbarAsync(34, "MARCADOR VACÍO PREEXISTENTE BLOQUEA REINTENTO", async () =>
    {
        Guid op = Guid.NewGuid();
        await File.WriteAllTextAsync(ProductionImportAttemptService.ObtenerRuta(rutaEstado, op), "");
        await ProductionImportAttemptService.RegistrarAsync(rutaEstado, op, backupId);
    }, typeof(InvalidOperationException), "Ya existe un intento de importación registrado para esta operación.");

    await ProbarAsync(35, "MARCADOR ALTERADO BLOQUEA REINTENTO", async () =>
    {
        Guid op = Guid.NewGuid();
        await File.WriteAllTextAsync(ProductionImportAttemptService.ObtenerRuta(rutaEstado, op), "contenido alterado");
        await ProductionImportAttemptService.RegistrarAsync(rutaEstado, op, backupId);
    }, typeof(InvalidOperationException), "Ya existe un intento de importación registrado para esta operación.");

    await SembrarEstadoAsync(EstadoPrueba("PreventiveBackupVerified"));
    await File.AppendAllTextAsync(rutaBackup, "\nALTERACIÓN SIMULADA");
    await ProbarAsync(36, "RECHAZAR BACKUP PREVENTIVO ALTERADO", async () =>
    {
        await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId);
    }, typeof(InvalidOperationException), "El backup preventivo fue modificado después de su verificación.");
    await File.WriteAllTextAsync(rutaBackup, "Archivo temporal para verificar SHA-256; no es un respaldo real.");

    await SembrarEstadoAsync(EstadoPrueba("Completed", bloqueo: true, importacion: true));
    await ProbarAsync(37, "COMPLETED INCONSISTENTE NO LIBERA OPERACIÓN", async () =>
    {
        await ProductionRestoreStateService.VerificarSinPendientesAsync(rutaEstado);
    }, typeof(InvalidOperationException),
       $"Existe una restauración de producción pendiente o un estado final inconsistente. Operación: {operationId}. Fase: Completed. Se requiere revisión administrativa.");

    await SembrarEstadoAsync(EstadoPrueba("Completed", bloqueo: true, importacion: true,
        importacionCompletada: true, verificacion: true, desbloqueo: true));
    await ProbarExitoAsync(38, "COMPLETED CONSISTENTE ADMITE NUEVA OPERACIÓN", async () =>
    {
        await ProductionRestoreStateService.VerificarSinPendientesAsync(rutaEstado);
        await ProductionRestoreStateService.CrearAsync(rutaEstado, new()
        {
            OperationId = Guid.NewGuid(),
            BackupId = Guid.NewGuid(),
            Phase = "PreventiveBackupStarted"
        });
        var actual = await ProductionRestoreStateService.LeerAsync(rutaEstado);
        if (actual?.Phase != "PreventiveBackupStarted")
            throw new InvalidOperationException("No se creó la nueva operación.");
    });

    await SembrarEstadoAsync(EstadoPrueba("RequiresRecovery", bloqueo: true,
        importacion: true, error: "Interrupción simulada"));
    await ProbarAsync(39, "RECUPERACIÓN PENDIENTE BLOQUEA NUEVA OPERACIÓN", async () =>
    {
        await ProductionRestoreStateService.CrearAsync(rutaEstado, new()
        {
            OperationId = Guid.NewGuid(),
            BackupId = Guid.NewGuid(),
            Phase = "PreventiveBackupStarted"
        });
    }, typeof(InvalidOperationException), "Existe una restauración pendiente.");

    await SembrarEstadoAsync(EstadoPrueba("ImportStarted", bloqueo: true, importacion: true));
    await ProbarExitoAsync(40, "REINICIO SIMULADO: MARCADOR Y ESTADO IMPIDEN REPETICIÓN", async () =>
    {
        Guid op = Guid.NewGuid();
        await ProductionImportAttemptService.RegistrarAsync(rutaEstado, op, backupId);
        bool marcadorRechazado = false, continuacionRechazada = false;
        try { await ProductionImportAttemptService.RegistrarAsync(rutaEstado, op, backupId); }
        catch (InvalidOperationException) { marcadorRechazado = true; }
        try { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }
        catch (InvalidOperationException) { continuacionRechazada = true; }
        if (!marcadorRechazado || !continuacionRechazada)
            throw new InvalidOperationException("Una protección permitió reintentar.");
    });


    // PRUEBAS 41-45: simulaciones de estados persistentes; no se inicia MariaDB.
    await SembrarEstadoAsync(EstadoPrueba("ApplicationIsolated", bloqueo: true));
    await ProbarExitoAsync(41, "INTERRUPCIÓN ANTES DE IMPORTSTARTED", async () =>
    {
        await ProductionRestoreStateService.ActualizarAsync(rutaEstado,
            EstadoPrueba("RequiresRecovery", bloqueo: true,
                error: "Interrupción simulada antes de importar."));
        var actual = await ProductionRestoreStateService.LeerAsync(rutaEstado);
        if (actual?.Phase != "RequiresRecovery" || !actual.ApplicationLockAttempted ||
            actual.ImportStarted || actual.ImportCompleted || actual.ApplicationUnlocked)
            throw new InvalidOperationException("No se conservó el estado anterior a la importación.");
    });

    await SembrarEstadoAsync(EstadoPrueba("ImportStarted", bloqueo: true, importacion: true));
    await ProbarExitoAsync(42, "INTERRUPCIÓN DESPUÉS DE IMPORTSTARTED", async () =>
    {
        await ProductionRestoreStateService.ActualizarAsync(rutaEstado,
            EstadoPrueba("RequiresRecovery", bloqueo: true, importacion: true,
                error: "Interrupción simulada durante la importación."));
        var actual = await ProductionRestoreStateService.LeerAsync(rutaEstado);
        if (actual?.Phase != "RequiresRecovery" || !actual.ImportStarted ||
            actual.ImportCompleted || actual.ApplicationUnlocked)
            throw new InvalidOperationException("Se perdió el indicador de importación iniciada.");
        bool rechazado = false;
        try { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }
        catch (InvalidOperationException) { rechazado = true; }
        if (!rechazado) throw new InvalidOperationException("Se permitió continuar una importación incierta.");
    });

    await SembrarEstadoAsync(EstadoPrueba("ImportCompleted", bloqueo: true,
        importacion: true, importacionCompletada: true));
    await ProbarExitoAsync(43, "FALLO DURANTE VERIFICACIÓN POSTERIOR", async () =>
    {
        await ProductionRestoreStateService.ActualizarAsync(rutaEstado,
            EstadoPrueba("RequiresRecovery", bloqueo: true, importacion: true,
                importacionCompletada: true, error: "Verificación posterior fallida (simulada)."));
        var actual = await ProductionRestoreStateService.LeerAsync(rutaEstado);
        if (actual?.Phase != "RequiresRecovery" || !actual.ImportCompleted ||
            actual.VerificationCompleted || actual.ApplicationUnlocked)
            throw new InvalidOperationException("La verificación fallida quedó marcada como completada.");
    });

    await SembrarEstadoAsync(EstadoPrueba("VerificationCompleted", bloqueo: true,
        importacion: true, importacionCompletada: true, verificacion: true));
    await ProbarExitoAsync(44, "INTERRUPCIÓN DURANTE DESBLOQUEO", async () =>
    {
        await ProductionRestoreStateService.ActualizarAsync(rutaEstado,
            EstadoPrueba("RequiresRecovery", bloqueo: true, importacion: true,
                importacionCompletada: true, verificacion: true,
                error: "Desbloqueo incierto (simulado); requiere revisión."));
        var actual = await ProductionRestoreStateService.LeerAsync(rutaEstado);
        if (actual?.Phase != "RequiresRecovery" || !actual.VerificationCompleted ||
            actual.ApplicationUnlocked)
            throw new InvalidOperationException("Se marcó desbloqueo sin confirmación.");
        bool rechazado = false;
        try { await ProductionRestoreStateService.VerificarSinPendientesAsync(rutaEstado); }
        catch (InvalidOperationException) { rechazado = true; }
        if (!rechazado) throw new InvalidOperationException("Se liberó una operación de desbloqueo incierto.");
    });

    await ProbarExitoAsync(45, "REINICIO SIMULADO CON RECUPERACIÓN PENDIENTE", async () =>
    {
        // Releer desde disco simula una nueva instancia; no se lanza otro proceso.
        var recargado = await ProductionRestoreStateService.LeerAsync(rutaEstado);
        if (recargado?.Phase != "RequiresRecovery" || string.IsNullOrWhiteSpace(recargado.Error))
            throw new InvalidOperationException("No persistió la recuperación pendiente.");
        bool nuevaRechazada = false, continuacionRechazada = false;
        try
        {
            await ProductionRestoreStateService.CrearAsync(rutaEstado, new()
            {
                OperationId = Guid.NewGuid(),
                BackupId = Guid.NewGuid(),
                Phase = "PreventiveBackupStarted"
            });
        }
        catch (InvalidOperationException) { nuevaRechazada = true; }
        try { await ProductionRestoreStateService.ValidarContinuacionAsync(rutaEstado, operationId, backupId); }
        catch (InvalidOperationException) { continuacionRechazada = true; }
        if (!nuevaRechazada || !continuacionRechazada)
            throw new InvalidOperationException("El reinicio permitió continuar o crear una operación.");
    });

}
catch (Exception ex)
{
    Console.Error.WriteLine("ERROR GENERAL: " + ex);
    fallidas++;
}
finally
{
    try { Directory.Delete(directorioTemporal, recursive: true); }
    catch (Exception ex) { Console.Error.WriteLine("Error limpiando archivos temporales: " + ex.Message); fallidas++; }
}

Console.WriteLine("\n================================");
Console.WriteLine("RESUMEN DE PRUEBAS 7 A 45");
Console.WriteLine("================================");
Console.WriteLine($"Pruebas aprobadas: {aprobadas}");
Console.WriteLine($"Pruebas fallidas:  {fallidas}");
Console.WriteLine(aprobadas == 39 && fallidas == 0
    ? "RESULTADO: TODAS LAS PRUEBAS APROBADAS."
    : "RESULTADO: EXISTEN PRUEBAS FALLIDAS.");
Environment.ExitCode = aprobadas == 39 && fallidas == 0 ? 0 : 1;
