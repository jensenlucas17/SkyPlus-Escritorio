namespace SkyPlus.RestoreWorker.Services;

public static class ProductionRestoreService
{
    public static Task PrepararAsync(
        string rutaBackup,
        string baseDatos)
    {
        if (!string.Equals(
            baseDatos,
            "skyplus",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "La base de datos de destino no es válida.");
        }

        if (!File.Exists(rutaBackup))
        {
            throw new FileNotFoundException(
                "No se encontró el backup seleccionado.",
                rutaBackup);
        }

        var archivo = new FileInfo(rutaBackup);

        if (archivo.Length == 0)
        {
            throw new InvalidOperationException(
                "El archivo de backup está vacío.");
        }

        if (!string.Equals(
            archivo.Extension,
            ".sql",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "El backup debe ser un archivo SQL.");
        }

        Console.WriteLine(
            "Preparación de restauración de producción correcta.");

        Console.WriteLine(
            "Base de destino: " + baseDatos);

        Console.WriteLine(
            "Archivo: " + archivo.Name);

        Console.WriteLine(
            "Tamaño: " + archivo.Length + " bytes");

        Console.WriteLine(
            "No se modificó la base de datos.");

        return Task.CompletedTask;
    }
}
