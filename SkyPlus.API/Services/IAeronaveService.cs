using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IAeronaveService
    {
        Task<List<AeronaveResponse>> ObtenerTodosAsync();

        Task<AeronaveResponse?> ObtenerPorIdAsync(int id);

        Task<AeronaveResponse?> CrearAsync(
            CrearAeronaveRequest request);

        Task<AeronaveResponse?> ActualizarAsync(
            int id,
            ActualizarAeronaveRequest request);

        Task<bool> EliminarAsync(int id);
    }
}