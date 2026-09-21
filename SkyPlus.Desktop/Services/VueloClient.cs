using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class VueloClient
    {
        private readonly ApiClient _apiClient;

        public VueloClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<VueloResponse>?> ObtenerTodosAsync()
        {
            return await _apiClient.GetAsync<List<VueloResponse>>(
                "api/Vuelos");
        }

        public async Task<VueloResponse?> ObtenerPorIdAsync(int id)
        {
            return await _apiClient.GetAsync<VueloResponse>(
                $"api/Vuelos/{id}");
        }

        public async Task<VueloResponse?> CrearAsync(
            string numeroVuelo,
            int idAeronave,
            int idUsuarioOperador,
            int idLugarOrigen,
            int idLugarDestino,
            DateTime salida,
            DateTime llegada,
            string estadoVuelo,
            decimal tarifa)
        {
            var request = new
            {
                NumeroVuelo = numeroVuelo,
                IdAeronave = idAeronave,
                IdUsuarioOperador = idUsuarioOperador,
                IdLugarOrigen = idLugarOrigen,
                IdLugarDestino = idLugarDestino,
                Salida = salida,
                Llegada = llegada,
                EstadoVuelo = estadoVuelo,
                Tarifa = tarifa
            };

            return await _apiClient.PostAsync<object, VueloResponse>(
                "api/Vuelos",
                request);
        }

        public async Task<VueloResponse?> ActualizarAsync(
            int id,
            string numeroVuelo,
            int idAeronave,
            int idUsuarioOperador,
            int idLugarOrigen,
            int idLugarDestino,
            DateTime salida,
            DateTime llegada,
            string estadoVuelo,
            decimal tarifa)
        {
            var request = new
            {
                NumeroVuelo = numeroVuelo,
                IdAeronave = idAeronave,
                IdUsuarioOperador = idUsuarioOperador,
                IdLugarOrigen = idLugarOrigen,
                IdLugarDestino = idLugarDestino,
                Salida = salida,
                Llegada = llegada,
                EstadoVuelo = estadoVuelo,
                Tarifa = tarifa
            };

            return await _apiClient.PutAsync<object, VueloResponse>(
                $"api/Vuelos/{id}",
                request);
        }

        public async Task EliminarAsync(int id)
        {
            await _apiClient.DeleteAsync(
                $"api/Vuelos/{id}");
        }
    }
}