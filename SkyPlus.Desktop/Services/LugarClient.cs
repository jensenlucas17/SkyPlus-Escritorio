using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class LugarClient
    {
        private readonly ApiClient _apiClient;

        public LugarClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<LugarResponse>?> ObtenerTodosAsync()
        {
            return await _apiClient.GetAsync<List<LugarResponse>>(
                "api/Lugares");
        }

        public async Task<LugarResponse?> ObtenerPorIdAsync(int id)
        {
            return await _apiClient.GetAsync<LugarResponse>(
                $"api/Lugares/{id}");
        }

        public async Task<LugarResponse?> CrearAsync(
            string codigoIata,
            string nombre,
            string ciudad,
            string pais)
        {
            var request = new
            {
                CodigoIata = codigoIata,
                Nombre = nombre,
                Ciudad = ciudad,
                Pais = pais
            };

            return await _apiClient.PostAsync<object, LugarResponse>(
                "api/Lugares",
                request);
        }

        public async Task<LugarResponse?> ActualizarAsync(
            int id,
            string codigoIata,
            string nombre,
            string ciudad,
            string pais)
        {
            var request = new
            {
                CodigoIata = codigoIata,
                Nombre = nombre,
                Ciudad = ciudad,
                Pais = pais
            };

            return await _apiClient.PutAsync<object, LugarResponse>(
                $"api/Lugares/{id}",
                request);
        }

        public async Task EliminarAsync(int id)
        {
            await _apiClient.DeleteAsync(
                $"api/Lugares/{id}");
        }
    }
}