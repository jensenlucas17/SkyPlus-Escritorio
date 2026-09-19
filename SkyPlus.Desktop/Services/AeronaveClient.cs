using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class AeronaveClient
    {
        private readonly ApiClient _apiClient;

        public AeronaveClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<AeronaveResponse>?> ObtenerTodosAsync()
        {
            return await _apiClient.GetAsync<List<AeronaveResponse>>(
                "api/Aeronaves");
        }

        public async Task<AeronaveResponse?> ObtenerPorIdAsync(int id)
        {
            return await _apiClient.GetAsync<AeronaveResponse>(
                $"api/Aeronaves/{id}");
        }

        public async Task<AeronaveResponse?> CrearAsync(
            string matricula,
            string modelo)
        {
            var request = new
            {
                Matricula = matricula,
                Modelo = modelo
            };

            return await _apiClient.PostAsync<object, AeronaveResponse>(
                "api/Aeronaves",
                request);
        }

        public async Task<AeronaveResponse?> ActualizarAsync(
            int id,
            string matricula,
            string modelo)
        {
            var request = new
            {
                Matricula = matricula,
                Modelo = modelo
            };

            return await _apiClient.PutAsync<object, AeronaveResponse>(
                $"api/Aeronaves/{id}",
                request);
        }

        public async Task EliminarAsync(int id)
        {
            await _apiClient.DeleteAsync(
                $"api/Aeronaves/{id}");
        }
    }
}