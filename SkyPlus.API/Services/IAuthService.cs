using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}