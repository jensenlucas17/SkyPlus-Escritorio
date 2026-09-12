using System.Windows.Forms;

namespace SkyPlus.Desktop.Services
{
    public static class ApiTest
    {
        public static async Task EjecutarAsync()
        {
            try
            {
                var usuarioClient = new UsuarioClient();

                // Colocá aquí el ID del usuario de prueba
                int idUsuario = 3;

                await usuarioClient.EliminarAsync(idUsuario);

                MessageBox.Show(
                    "✅ USUARIO DESACTIVADO CORRECTAMENTE\n\n" +
                    $"ID del usuario: {idUsuario}\n" +
                    "Estado esperado: Inactivo",
                    "Prueba SkyPlus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ ERROR AL DESACTIVAR USUARIO\n\n{ex.Message}",
                    "Prueba SkyPlus",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}