using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class PasajeroClient
    {
        private readonly ApiClient _apiClient;

        public PasajeroClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<PasajeroResponse>?> ObtenerTodosAsync()
        {
            return await _apiClient.GetAsync<List<PasajeroResponse>>(
                "api/Pasajeros");
        }

        public async Task<PasajeroResponse?> ObtenerPorIdAsync(int id)
        {
            return await _apiClient.GetAsync<PasajeroResponse>(
                $"api/Pasajeros/{id}");
        }

        public async Task<PasajeroResponse?> CrearAsync(
            string nombre,
            string apellido,
            string documento,
            string nacionalidad,
            string email,
            string telefono)
        {
            var request = new
            {
                Nombre = nombre,
                Apellido = apellido,
                Documento = documento,
                Nacionalidad = nacionalidad,
                Email = email,
                Telefono = telefono
            };

            return await _apiClient.PostAsync<object, PasajeroResponse>(
                "api/Pasajeros",
                request);
        }

        public async Task<PasajeroResponse?> ActualizarAsync(
            int id,
            string nombre,
            string apellido,
            string documento,
            string nacionalidad,
            string email,
            string telefono)
        {
            var request = new
            {
                Nombre = nombre,
                Apellido = apellido,
                Documento = documento,
                Nacionalidad = nacionalidad,
                Email = email,
                Telefono = telefono
            };

            return await _apiClient.PutAsync<object, PasajeroResponse>(
                $"api/Pasajeros/{id}",
                request);
        }

        public async Task EliminarAsync(int id)
        {
            await _apiClient.DeleteAsync(
                $"api/Pasajeros/{id}");
        }
    }
}