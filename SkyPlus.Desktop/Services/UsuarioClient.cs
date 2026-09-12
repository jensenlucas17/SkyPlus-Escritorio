using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class UsuarioClient
    {
        private readonly ApiClient _apiClient;

        public UsuarioClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<UsuarioResponse>?> ObtenerTodosAsync()
        {
            return await _apiClient.GetAsync<List<UsuarioResponse>>(
                "api/Usuarios");
        }

        public async Task<UsuarioResponse?> ObtenerPorIdAsync(int id)
        {
            return await _apiClient.GetAsync<UsuarioResponse>(
                $"api/Usuarios/{id}");
        }

        public async Task<UsuarioResponse?> CrearAsync(
            CrearUsuarioRequest request)
        {
            return await _apiClient.PostAsync
                <CrearUsuarioRequest, UsuarioResponse>(
                "api/Usuarios",
                request);
        }

        public async Task<UsuarioResponse?> ActualizarAsync(
            int id,
            ActualizarUsuarioRequest request)
        {
            return await _apiClient.PutAsync
                <ActualizarUsuarioRequest, UsuarioResponse>(
                $"api/Usuarios/{id}",
                request);
        }

        public async Task EliminarAsync(int id)
        {
            await _apiClient.DeleteAsync($"api/Usuarios/{id}");
        }
    }
}