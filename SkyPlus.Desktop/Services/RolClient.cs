using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class RolClient
    {
        private readonly ApiClient _apiClient;

        public RolClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<RolResponse>?> ObtenerTodosAsync()
        {
            return await _apiClient.GetAsync<List<RolResponse>>(
                "api/Roles");
        }
    }
}