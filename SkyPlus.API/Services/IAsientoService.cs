using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IAsientoService
    {
        Task<List<AsientoResponse>> ObtenerPorVueloAsync(
            int idVuelo);

        Task<AsientoResponse?> ObtenerPorIdAsync(
            int id);

        Task<bool> GenerarAsientosAsync(
            int idVuelo);

        Task<AsientoResponse?> ActualizarEstadoAsync(
            int id,
            ActualizarEstadoAsientoRequest request);
    }
}