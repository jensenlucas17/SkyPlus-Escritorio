using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IPasajeroService
    {
        Task<List<PasajeroResponse>> ObtenerTodosAsync();

        Task<PasajeroResponse?> ObtenerPorIdAsync(int id);

        Task<PasajeroResponse?> CrearAsync(
            CrearPasajeroRequest request);

        Task<PasajeroResponse?> ActualizarAsync(
            int id,
            ActualizarPasajeroRequest request);

        Task<bool> EliminarAsync(int id);
    }
}