using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class ReservaClient
    {
        private readonly ApiClient _apiClient;

        public ReservaClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<ReservaResponse>?> ObtenerTodosAsync()
        {
            return await _apiClient.GetAsync<List<ReservaResponse>>(
                "api/Reservas");
        }

        public async Task<ReservaResponse?> ObtenerPorIdAsync(
            int id)
        {
            return await _apiClient.GetAsync<ReservaResponse>(
                $"api/Reservas/{id}");
        }

        public async Task<List<ReservaResponse>?> ObtenerPorPasajeroAsync(
            int idPasajero)
        {
            return await _apiClient.GetAsync<List<ReservaResponse>>(
                $"api/Reservas/pasajero/{idPasajero}");
        }

        public async Task<ReservaResponse?> CrearAsync(
            int idPasajero,
            int idVuelo,
            int idAsiento,
            int idUsuarioAgente)
        {
            var request = new
            {
                IdPasajero = idPasajero,
                IdVuelo = idVuelo,
                IdAsiento = idAsiento,
                IdUsuarioAgente = idUsuarioAgente
            };

            return await _apiClient.PostAsync<object, ReservaResponse>(
                "api/Reservas",
                request);
        }

        public async Task CancelarAsync(int id)
        {
            await _apiClient.PutAsync<object, object>(
                $"api/Reservas/{id}/cancelar",
                new { });
        }
    }
}