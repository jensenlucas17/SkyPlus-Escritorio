using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class AsientoClient
    {
        private readonly ApiClient _apiClient;

        public AsientoClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<AsientoResponse>?> ObtenerPorVueloAsync(
            int idVuelo)
        {
            return await _apiClient.GetAsync<List<AsientoResponse>>(
                $"api/Asientos/vuelo/{idVuelo}");
        }

        public async Task<AsientoResponse?> ObtenerPorIdAsync(
            int id)
        {
            return await _apiClient.GetAsync<AsientoResponse>(
                $"api/Asientos/{id}");
        }

        public async Task GenerarAsientosAsync(
            int idVuelo)
        {
            await _apiClient.PostAsync<object, object>(
                $"api/Asientos/generar/{idVuelo}",
                new { });
        }

        public async Task<AsientoResponse?> ActualizarEstadoAsync(
            int id,
            string estado)
        {
            var request = new
            {
                Estado = estado
            };

            return await _apiClient.PutAsync<object, AsientoResponse>(
                $"api/Asientos/{id}/estado",
                request);
        }
    }
}