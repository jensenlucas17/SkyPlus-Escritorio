
using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IPagoService
    {
        Task<List<PagoResponse>> ObtenerTodosAsync();
        Task<PagoResponse?> ObtenerPorIdAsync(int id);
        Task<PagoResponse?> CrearAsync(
            CrearPagoRequest request,
            int idUsuarioAutenticado);
        Task<PagoResponse?> CrearAdicionalAsync(
            CrearPagoRequest request,
            int idUsuarioAutenticado);
        Task<bool> AnularAsync(int id);
    }
}
