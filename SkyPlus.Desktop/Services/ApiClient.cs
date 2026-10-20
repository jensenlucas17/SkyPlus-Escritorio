using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SkyPlus.Desktop.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7198/")
            };
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            await VerificarRespuestaAsync(response);

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);

            await VerificarRespuestaAsync(response);

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);

            await VerificarRespuestaAsync(response);

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);

            await VerificarRespuestaAsync(response);
        }

        public async Task PatchAsync(string endpoint)
        {
            using var response = await _httpClient.PatchAsync(endpoint, null);
            await VerificarRespuestaAsync(response);
        }

        private static async Task VerificarRespuestaAsync(
    HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            var contenido = await response.Content.ReadAsStringAsync();

            string mensaje = $"Error HTTP {(int)response.StatusCode}.";

            try
            {
                using var json = JsonDocument.Parse(contenido);

                if (json.RootElement.TryGetProperty(
                    "mensaje", out var propiedad))
                {
                    mensaje = propiedad.GetString() ?? mensaje;
                }
            }
            catch (JsonException)
            {
                // La respuesta no contiene JSON válido.
            }

            throw new HttpRequestException(
                mensaje,
                null,
                response.StatusCode);
        }
    }
}