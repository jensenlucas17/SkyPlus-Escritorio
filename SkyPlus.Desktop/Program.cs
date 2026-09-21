using SkyPlus.Desktop.Models;
using System;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // SOLO PARA PROBAR - borrar antes de commitear
            /**SesionUsuario.IdUsuario = 1;
            SesionUsuario.Nombre = "Prueba";
            SesionUsuario.Apellido = "Local";
            SesionUsuario.Rol = "Administrador";**/ // probá también "Gerente", "Agente de Reservas", etc.
            Application.Run(new frmUsuarios());
        }
    }
}