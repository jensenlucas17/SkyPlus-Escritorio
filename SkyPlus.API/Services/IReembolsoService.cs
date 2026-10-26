
using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IReembolsoService
    {
        Task<List<ReembolsoResponse>> ObtenerTodosAsync();
        Task<ReembolsoResponse?> ObtenerPorIdAsync(int id);
        Task<ReembolsoResponse> CrearAsync(CrearReembolsoRequest request);
    }
}
