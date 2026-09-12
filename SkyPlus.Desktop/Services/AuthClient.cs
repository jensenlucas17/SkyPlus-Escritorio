using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class AuthClient
    {
        private readonly ApiClient _apiClient;

        public AuthClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<LoginResponse?> LoginAsync(
            string email,
            string password)
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            return await _apiClient.PostAsync<LoginRequest, LoginResponse>(
                "api/Auth/login",
                request);
        }
    }
}