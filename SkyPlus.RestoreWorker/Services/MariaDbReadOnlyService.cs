
using MySqlConnector;

namespace SkyPlus.RestoreWorker.Services;

public static class MariaDbReadOnlyService
{
    public static async Task<bool> ObtenerEstadoAsync(
        string archivoCredenciales)
    {
        var connectionString =
            CrearConexion(archivoCredenciales);

        await using var conexion =
            new MySqlConnection(connectionString);

        await conexion.OpenAsync();

        await using var comando = new MySqlCommand(
            "SELECT @@GLOBAL.read_only;",
            conexion);

        object? resultado = await comando.ExecuteScalarAsync();

        string estado = resultado?.ToString()?.Trim()
            ?? throw new InvalidOperationException(
                "MariaDB no devolvió el estado de read_only.");

        if (estado.Equals("ON", StringComparison.OrdinalIgnoreCase) ||
            estado == "1")
        {
            return true;
        }

        if (estado.Equals("OFF", StringComparison.OrdinalIgnoreCase) ||
            estado == "0")
        {
            return false;
        }

        throw new InvalidOperationException(
            $"Valor inesperado de read_only: {estado}");
    }

    public static async Task ActivarAsync(
        string archivoCredenciales)
    {
        if (await ObtenerEstadoAsync(archivoCredenciales))
            throw new InvalidOperationException(
                "MariaDB ya está en modo solo lectura.");

        await CambiarEstadoAsync(
            archivoCredenciales, true);

        if (!await ObtenerEstadoAsync(archivoCredenciales))
            throw new InvalidOperationException(
                "No se pudo confirmar la activación de read_only.");
    }

    public static async Task DesactivarAsync(
        string archivoCredenciales)
    {
        await CambiarEstadoAsync(
            archivoCredenciales, false);

        if (await ObtenerEstadoAsync(archivoCredenciales))
            throw new InvalidOperationException(
                "No se pudo confirmar la desactivación de read_only.");
    }

    private static async Task CambiarEstadoAsync(
        string archivoCredenciales,
        bool activar)
    {
        var connectionString =
            CrearConexion(archivoCredenciales);

        await using var conexion =
            new MySqlConnection(connectionString);

        await conexion.OpenAsync();

        string sql = activar
            ? "SET GLOBAL read_only = ON;"
            : "SET GLOBAL read_only = OFF;";

        await using var comando =
            new MySqlCommand(sql, conexion);

        await comando.ExecuteNonQueryAsync();
    }

    private static string CrearConexion(
        string archivoCredenciales)
    {
        if (!File.Exists(archivoCredenciales))
            throw new FileNotFoundException(
                "No existe maintenance.cnf.",
                archivoCredenciales);

        var valores = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (string lineaOriginal
                 in File.ReadAllLines(archivoCredenciales))
        {
            string linea = lineaOriginal.Trim();

            if (string.IsNullOrWhiteSpace(linea) ||
                linea.StartsWith("#") ||
                linea.StartsWith(";") ||
                linea.StartsWith("["))
                continue;

            int posicion = linea.IndexOf('=');

            if (posicion <= 0)
                continue;

            string clave = linea[..posicion].Trim();
            string valor = linea[(posicion + 1)..].Trim();

            valores[clave] = valor;
        }

        foreach (string clave in new[]
                 { "host", "port", "user", "password" })
        {
            if (!valores.ContainsKey(clave))
                throw new InvalidOperationException(
                    $"Falta '{clave}' en maintenance.cnf.");
        }

        var builder = new MySqlConnectionStringBuilder
        {
            Server = valores["host"],
            Port = uint.Parse(valores["port"]),
            UserID = valores["user"],
            Password = valores["password"],
            ConnectionTimeout = 10,
            Pooling = false
        };

        if (!string.Equals(
            builder.UserID,
            "skyplus_maintenance",
            StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "El usuario de mantenimiento no es válido.");
        }

        return builder.ConnectionString;
    }
}
