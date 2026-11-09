namespace SkyPlus.RestoreWorker.Services;

public sealed class RestoreExecutionLock : IDisposable
{
    private readonly FileStream _archivo;
    private bool _disposed;

    private RestoreExecutionLock(FileStream archivo)
    {
        _archivo = archivo;
    }

    public static RestoreExecutionLock Adquirir(string ruta)
    {
        string? directorio = Path.GetDirectoryName(ruta);

        if (string.IsNullOrWhiteSpace(directorio))
            throw new InvalidOperationException(
                "La ruta del bloqueo no es válida.");

        Directory.CreateDirectory(directorio);

        try
        {
            var archivo = new FileStream(
                ruta,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);

            return new RestoreExecutionLock(archivo);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException(
                "No se puede iniciar RestoreWorker: " +
                "otra instancia podría estar ejecutándose " +
                "o el archivo de bloqueo no está disponible.",
                ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _archivo.Dispose();
        _disposed = true;
    }
}
