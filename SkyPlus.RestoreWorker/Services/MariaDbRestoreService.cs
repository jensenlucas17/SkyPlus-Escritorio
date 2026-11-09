using System.Diagnostics;
using System.Text;

namespace SkyPlus.RestoreWorker.Services;

public static class MariaDbRestoreService
{
    private const string BaseValidacion = "skyplus_restore_validacion";
    private const string UsuarioValidacion = "skyplus_validacion@localhost";

    public static async Task VerificarConexionAsync(string mariaDbBinPath, string credentialsFile, CancellationToken ct = default)
    {
        string ejecutable = ValidarConfiguracion(mariaDbBinPath, credentialsFile);
        var inicio = CrearInicio(ejecutable, credentialsFile);
        inicio.ArgumentList.Add("--batch");
        inicio.ArgumentList.Add("--skip-column-names");
        inicio.ArgumentList.Add("--execute=SELECT CURRENT_USER(), DATABASE()");
        inicio.ArgumentList.Add(BaseValidacion);
        string salida = await EjecutarConSalidaAsync(new Process { StartInfo = inicio }, ct);
        string[] columnas = salida.Trim().Split('\t');
        if (columnas.Length != 2 ||
            !string.Equals(columnas[0], UsuarioValidacion, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(columnas[1], BaseValidacion, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("La conexión no corresponde al usuario o base de validación esperados.");
        Console.WriteLine("Conexión de validación comprobada.");
        Console.WriteLine("Usuario: " + columnas[0]);
        Console.WriteLine("Base: " + columnas[1]);
    }

    public static async Task VerificarIntegridadAsync(string mariaDbBinPath, string credentialsFile, CancellationToken ct = default)
    {
        await VerificarConexionAsync(mariaDbBinPath, credentialsFile, ct);
        string ejecutable = ValidarConfiguracion(mariaDbBinPath, credentialsFile);

        async Task<string> ConsultarAsync(string sql)
        {
            var inicio = CrearInicio(ejecutable, credentialsFile);
            inicio.ArgumentList.Add("--batch");
            inicio.ArgumentList.Add("--skip-column-names");
            inicio.ArgumentList.Add("--execute=" + sql);
            inicio.ArgumentList.Add(BaseValidacion);
            return (await EjecutarConSalidaAsync(new Process { StartInfo = inicio }, ct)).Trim();
        }

        string tablasTexto = await ConsultarAsync(
            "SELECT COUNT(*) FROM information_schema.tables " +
            "WHERE table_schema = 'skyplus_restore_validacion' AND table_type = 'BASE TABLE'");
        if (!int.TryParse(tablasTexto, out int tablas) || tablas != 16)
            throw new InvalidOperationException($"Se esperaban 16 tablas; se encontraron: {tablasTexto}.");

        // Cada fila representa una columna de una clave foránea. Las claves compuestas
        // se agrupan por nombre para comprobar conjuntamente todas sus columnas.
        string relacionesTexto = await ConsultarAsync(
            "SELECT constraint_name, table_name, column_name, referenced_table_name, referenced_column_name " +
            "FROM information_schema.key_column_usage " +
            "WHERE table_schema = 'skyplus_restore_validacion' " +
            "AND referenced_table_name IS NOT NULL " +
            "ORDER BY table_name, constraint_name, ordinal_position");

        var filas = relacionesTexto.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.TrimEnd('\r').Split('\t'))
            .ToArray();
        if (filas.Any(x => x.Length != 5))
            throw new InvalidOperationException("La metadata de las claves foráneas tiene un formato inesperado.");

        var relaciones = filas.GroupBy(x => (Tabla: x[1], Restriccion: x[0])).ToArray();
        if (relaciones.Length != 22)
            throw new InvalidOperationException($"Se esperaban 22 claves foráneas; se encontraron {relaciones.Length}.");

        static string Identificador(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor) || valor.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '_')))
                throw new InvalidOperationException("Identificador inesperado en metadata SQL.");
            return "`" + valor + "`";
        }

        long totalHuerfanos = 0;
        foreach (var relacion in relaciones)
        {
            var columnas = relacion.ToArray();
            string tablaHija = Identificador(columnas[0][1]);
            string tablaPadre = Identificador(columnas[0][3]);
            if (columnas.Any(c => c[1] != columnas[0][1] || c[3] != columnas[0][3]))
                throw new InvalidOperationException("Clave foránea inconsistente: " + relacion.Key.Restriccion);

            string condicion = string.Join(" AND ", columnas.Select(c =>
                "h." + Identificador(c[2]) + " = p." + Identificador(c[4])));
            string noNulos = string.Join(" AND ", columnas.Select(c =>
                "h." + Identificador(c[2]) + " IS NOT NULL"));
            string sql = "SELECT COUNT(*) FROM " + Identificador(BaseValidacion) + "." + tablaHija +
                " h WHERE " + noNulos + " AND NOT EXISTS (SELECT 1 FROM " +
                Identificador(BaseValidacion) + "." + tablaPadre + " p WHERE " + condicion + ")";
            string resultado = await ConsultarAsync(sql);
            if (!long.TryParse(resultado, out long huerfanos))
                throw new InvalidOperationException("No se pudo interpretar el conteo de " + relacion.Key.Restriccion);
            totalHuerfanos += huerfanos;
            Console.WriteLine($"{relacion.Key.Restriccion}: {huerfanos} huérfanos");
        }

        Console.WriteLine($"Tablas: {tablas}; claves foráneas: {relaciones.Length}; huérfanos: {totalHuerfanos}.");
        if (totalHuerfanos != 0)
            throw new InvalidOperationException("Se encontraron registros huérfanos en la base de validación.");
        Console.WriteLine("Integridad referencial de validación: CORRECTA.");
    }

    public static async Task ImportarValidacionAsync(string mariaDbBinPath, string credentialsFile,
        string rutaBackup, string baseDatos, CancellationToken ct = default)
    {
        if (!string.Equals(baseDatos, BaseValidacion, StringComparison.Ordinal))
            throw new InvalidOperationException("Solo se permite importar en la base de validación.");
        string ejecutable = ValidarConfiguracion(mariaDbBinPath, credentialsFile);
        if (!File.Exists(rutaBackup))
            throw new FileNotFoundException("No se encontró el backup SQL.", rutaBackup);
        if (!string.Equals(Path.GetExtension(rutaBackup), ".sql", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("El archivo debe tener extensión .sql.");
        if (new FileInfo(rutaBackup).Length == 0)
            throw new InvalidOperationException("El archivo SQL está vacío.");

        await VerificarConexionAsync(mariaDbBinPath, credentialsFile, ct);
        var inicio = CrearInicio(ejecutable, credentialsFile);
        inicio.RedirectStandardInput = true;
        inicio.ArgumentList.Add(BaseValidacion);
        using var proceso = new Process { StartInfo = inicio };
        if (!proceso.Start())
            throw new InvalidOperationException("No se pudo iniciar MariaDB.");
        try
        {
            Task<string> errores = proceso.StandardError.ReadToEndAsync();
            Task<string> salida = proceso.StandardOutput.ReadToEndAsync();
            await using (var archivo = File.OpenRead(rutaBackup))
                await archivo.CopyToAsync(proceso.StandardInput.BaseStream, ct);
            proceso.StandardInput.Close();
            await proceso.WaitForExitAsync(ct);
            string error = await errores;
            await salida;
            if (proceso.ExitCode != 0)
                throw new InvalidOperationException(
                    "La importación SQL falló. La base de validación podría haber quedado parcialmente importada. " +
                    (string.IsNullOrWhiteSpace(error) ? "" : error.Trim()));
            Console.WriteLine("Importación de validación finalizada correctamente.");
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
            catch { }
            throw;
        }
    }

    private static string ValidarConfiguracion(string mariaDbBinPath, string credentialsFile)
    {
        string ejecutable = Path.Combine(mariaDbBinPath, "mariadb.exe");
        if (!File.Exists(ejecutable))
            throw new FileNotFoundException("No se encontró mariadb.exe.", ejecutable);
        if (!File.Exists(credentialsFile) ||
            !string.Equals(Path.GetFileName(credentialsFile), "validacion.cnf", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Se requiere el archivo validacion.cnf.");
        return ejecutable;
    }

    private static ProcessStartInfo CrearInicio(string ejecutable, string credentialsFile)
    {
        var inicio = new ProcessStartInfo
        {
            FileName = ejecutable,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        inicio.ArgumentList.Add($"--defaults-file={credentialsFile}");
        inicio.ArgumentList.Add("--default-character-set=utf8mb4");
        return inicio;
    }

    private static async Task<string> EjecutarConSalidaAsync(Process proceso, CancellationToken ct)
    {
        using (proceso)
        {
            if (!proceso.Start())
                throw new InvalidOperationException("No se pudo iniciar MariaDB.");
            try
            {
                Task<string> salida = proceso.StandardOutput.ReadToEndAsync();
                Task<string> errores = proceso.StandardError.ReadToEndAsync();
                await proceso.WaitForExitAsync(ct);
                string texto = await salida;
                string error = await errores;
                if (proceso.ExitCode != 0)
                    throw new InvalidOperationException("Falló la consulta de validación con MariaDB. " + error.Trim());
                return texto;
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
                catch { }
                throw;
            }
        }
    }
}