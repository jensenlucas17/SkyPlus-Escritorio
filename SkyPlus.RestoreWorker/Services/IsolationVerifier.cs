using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;

namespace SkyPlus.RestoreWorker.Services
{
    [SupportedOSPlatform("windows")]
    public static class IsolationVerifier
    {
        public static void VerificarProcesos()
        {
            string[] aplicaciones =
            {
                "SkyPlus.API",
                "SkyPlus.Desktop",
                "SkyPlus.BackupWorker"
            };

            using var buscador = new ManagementObjectSearcher(
                "SELECT Name, ProcessId, CommandLine FROM Win32_Process");

            foreach (ManagementObject proceso in buscador.Get())
            {
                string nombre = proceso["Name"]?.ToString() ?? "";
                string comando = proceso["CommandLine"]?.ToString() ?? "";

                bool procesoSkyPlus = aplicaciones.Any(app =>
                    nombre.Equals(app + ".exe",
                        StringComparison.OrdinalIgnoreCase) ||
                    (nombre.Equals("dotnet.exe",
                        StringComparison.OrdinalIgnoreCase) &&
                     comando.Contains(app,
                        StringComparison.OrdinalIgnoreCase)));

                if (procesoSkyPlus)
                {
                    throw new InvalidOperationException(
                        $"Se detectó un proceso activo de SkyPlus: {nombre} " +
                        $"(PID {proceso["ProcessId"]}). Cerrá la aplicación antes de continuar.");
                }
            }
        }
    }
}
