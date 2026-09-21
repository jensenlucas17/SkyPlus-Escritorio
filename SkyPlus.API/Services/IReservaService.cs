using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IReservaService
    {
        Task<List<ReservaResponse>> ObtenerTodosAsync();

        Task<ReservaResponse?> ObtenerPorIdAsync(
            int id);

        Task<List<ReservaResponse>> ObtenerPorPasajeroAsync(
            int idPasajero);

        Task<ReservaResponse?> CrearAsync(
            CrearReservaRequest request);

        Task<bool> CancelarAsync(
            int id);
    }
}