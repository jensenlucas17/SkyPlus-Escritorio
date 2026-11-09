using MySqlConnector;

namespace SkyPlus.RestoreWorker.Services;

public static class MariaDbIsolationVerifier
{
    public static async Task VerificarAsync(
        string archivoCredenciales,
        string baseDatos)
    {
        if (!File.Exists(archivoCredenciales))
            throw new FileNotFoundException(
                "No se encontró el archivo de credenciales de monitoreo.",
                archivoCredenciales);

        var credenciales = LeerCredenciales(archivoCredenciales);

        var builder = new MySqlConnectionStringBuilder
        {
            Server = credenciales["host"],
            Port = uint.Parse(credenciales["port"]),
            UserID = credenciales["user"],
            Password = credenciales["password"],
            Database = "information_schema",
            SslMode = MySqlSslMode.Preferred,
            ConnectionTimeout = 10
        };

        await using var conexion =
            new MySqlConnection(builder.ConnectionString);

        await conexion.OpenAsync();

        await using var comando =
            new MySqlCommand("SHOW FULL PROCESSLIST;", conexion);

        await using var lector =
            await comando.ExecuteReaderAsync();

        var conexiones = new List<string>();

        while (await lector.ReadAsync())
        {
            var usuario = lector["User"]?.ToString();
            var host = lector["Host"]?.ToString();
            var bd = lector["db"]?.ToString();
            var id = lector["Id"]?.ToString();
            var estado = lector["Command"]?.ToString();

            if (string.Equals(
                bd,
                baseDatos,
                StringComparison.OrdinalIgnoreCase))
            {
                conexiones.Add(
                    $"ID={id}, Usuario={usuario}, Host={host}, Estado={estado}");
            }
        }

        if (conexiones.Count > 0)
        {
            throw new InvalidOperationException(
                "Se detectaron conexiones abiertas a la base de datos " +
                $"'{baseDatos}':{Environment.NewLine}" +
                string.Join(Environment.NewLine, conexiones));
        }

        Console.WriteLine(
            $"Verificación correcta: no hay conexiones abiertas a '{baseDatos}'.");
    }

    private static Dictionary<string, string> LeerCredenciales(string ruta)
    {
        var valores = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var lineaOriginal in File.ReadAllLines(ruta))
        {
            var linea = lineaOriginal.Trim();

            if (string.IsNullOrWhiteSpace(linea) ||
                linea.StartsWith("#") ||
                linea.StartsWith(";") ||
                linea.StartsWith("["))
                continue;

            var separador = linea.IndexOf('=');

            if (separador <= 0)
                continue;

            var clave = linea[..separador].Trim();
            var valor = linea[(separador + 1)..].Trim();

            valores[clave] = valor;
        }

        foreach (var clave in new[] { "host", "port", "user", "password" })
        {
            if (!valores.ContainsKey(clave))
                throw new InvalidOperationException(
                    $"Falta '{clave}' en el archivo de monitoreo.");
        }

        return valores;
    }
}