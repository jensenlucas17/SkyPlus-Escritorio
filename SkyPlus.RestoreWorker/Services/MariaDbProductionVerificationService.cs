using System.Diagnostics;

namespace SkyPlus.RestoreWorker.Services;

/// <summary>
/// Verificación de solo lectura de la base de producción tras importar un respaldo.
/// No modifica datos, estados persistidos ni el bloqueo de la aplicación.
/// </summary>
public static class MariaDbProductionVerificationService
{
    private const string BaseProduccion = "skyplus";
    private const string UsuarioProduccion = "skyplus_restore_prod@localhost";
    private const int TablasEsperadas = 16;
    private const int RelacionesEsperadas = 22;

    public static async Task VerificarAsync(
        string mariaDbBinPath,
        string credentialsFile,
        CancellationToken cancellationToken = default)
    {
        string ejecutable = ValidarConfiguracion(mariaDbBinPath, credentialsFile);

        async Task<string> ConsultarAsync(string sql)
        {
            var inicio = new ProcessStartInfo
            {
                FileName = ejecutable,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            inicio.ArgumentList.Add($"--defaults-file={credentialsFile}");
            inicio.ArgumentList.Add("--default-character-set=utf8mb4");
            inicio.ArgumentList.Add("--batch");
            inicio.ArgumentList.Add("--skip-column-names");
            inicio.ArgumentList.Add("--execute=" + sql);
            inicio.ArgumentList.Add(BaseProduccion);

            using var proceso = new Process { StartInfo = inicio };
            if (!proceso.Start())
                throw new InvalidOperationException("No se pudo iniciar MariaDB.");

            try
            {
                Task<string> salida = proceso.StandardOutput.ReadToEndAsync();
                Task<string> errores = proceso.StandardError.ReadToEndAsync();
                await proceso.WaitForExitAsync(cancellationToken);
                string texto = await salida;
                string error = await errores;
                if (proceso.ExitCode != 0)
                    throw new InvalidOperationException(
                        "Falló la verificación de producción: " + error.Trim());
                return texto.Trim();
            }
            catch
            {
                try
                {
                    if (!proceso.HasExited)
                    {
                        proceso.Kill(entireProcessTree: true);
                        await proceso.WaitForExitAsync(CancellationToken.None);
                    }
                }
                catch { /* Conservar el error original. */ }
                throw;
            }
        }

        // No confiar solo en el nombre del archivo de credenciales.
        string identidad = await ConsultarAsync("SELECT CURRENT_USER(), DATABASE()");
        string[] datosIdentidad = identidad.Split('\t');
        if (datosIdentidad.Length != 2 ||
            !string.Equals(datosIdentidad[0], UsuarioProduccion, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(datosIdentidad[1], BaseProduccion, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "La conexión no corresponde al usuario y base de producción esperados.");

        string textoTablas = await ConsultarAsync(
            "SELECT COUNT(*) FROM information_schema.tables " +
            "WHERE table_schema = 'skyplus' AND table_type = 'BASE TABLE'");
        if (!int.TryParse(textoTablas, out int tablas) || tablas != TablasEsperadas)
            throw new InvalidOperationException(
                $"Se esperaban {TablasEsperadas} tablas en skyplus; se encontraron: {textoTablas}.");

        string textoRelaciones = await ConsultarAsync(
            "SELECT constraint_name, table_name, column_name, referenced_table_name, referenced_column_name " +
            "FROM information_schema.key_column_usage " +
            "WHERE table_schema = 'skyplus' AND referenced_table_name IS NOT NULL " +
            "ORDER BY table_name, constraint_name, ordinal_position");

        string[][] filas = textoRelaciones
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.TrimEnd('\r').Split('\t'))
            .ToArray();
        if (filas.Any(f => f.Length != 5))
            throw new InvalidOperationException("Metadata de claves foráneas inesperada.");

        var relaciones = filas.GroupBy(f => (Tabla: f[1], Restriccion: f[0])).ToArray();
        if (relaciones.Length != RelacionesEsperadas)
            throw new InvalidOperationException(
                $"Se esperaban {RelacionesEsperadas} claves foráneas; se encontraron {relaciones.Length}.");

        static string Identificador(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre) ||
                nombre.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '_')))
                throw new InvalidOperationException("Identificador SQL inesperado en metadata.");
            return "`" + nombre + "`";
        }

        long totalHuerfanos = 0;
        foreach (var relacion in relaciones)
        {
            string[][] columnas = relacion.ToArray();
            if (columnas.Any(c => c[1] != columnas[0][1] || c[3] != columnas[0][3]))
                throw new InvalidOperationException(
                    "Metadata inconsistente para la relación " + relacion.Key.Restriccion);

            string tablaHija = Identificador(columnas[0][1]);
            string tablaPadre = Identificador(columnas[0][3]);
            string condicion = string.Join(" AND ", columnas.Select(c =>
                "h." + Identificador(c[2]) + " = p." + Identificador(c[4])));
            string noNulos = string.Join(" AND ", columnas.Select(c =>
                "h." + Identificador(c[2]) + " IS NOT NULL"));

            string consulta =
                "SELECT COUNT(*) FROM `skyplus`." + tablaHija + " h WHERE " +
                noNulos + " AND NOT EXISTS (SELECT 1 FROM `skyplus`." +
                tablaPadre + " p WHERE " + condicion + ")";

            string resultado = await ConsultarAsync(consulta);
            if (!long.TryParse(resultado, out long huerfanos))
                throw new InvalidOperationException(
                    "Conteo de huérfanos inválido para " + relacion.Key.Restriccion);
            totalHuerfanos += huerfanos;
            Console.WriteLine($"{relacion.Key.Restriccion}: {huerfanos} huérfanos");
        }

        Console.WriteLine(
            $"Producción: {tablas} tablas; {relaciones.Length} claves foráneas; " +
            $"{totalHuerfanos} huérfanos.");

        if (totalHuerfanos != 0)
            throw new InvalidOperationException(
                "Se detectaron registros huérfanos en skyplus.");

        Console.WriteLine("Verificación referencial de producción: CORRECTA.");
    }

    private static string ValidarConfiguracion(string mariaDbBinPath, string credentialsFile)
    {
        string ejecutable = Path.Combine(mariaDbBinPath, "mariadb.exe");
        if (!File.Exists(ejecutable))
            throw new FileNotFoundException("No se encontró mariadb.exe.", ejecutable);
        if (!File.Exists(credentialsFile) ||
            !string.Equals(Path.GetFileName(credentialsFile), "restore-prod.cnf",
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Se requiere restore-prod.cnf.");
        return ejecutable;
    }
}
