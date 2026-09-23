using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IVueloService
    {
        Task<List<VueloResponse>> ObtenerTodosAsync();

        Task<VueloResponse?> ObtenerPorIdAsync(int id);

        Task<VueloResponse?> CrearAsync(
            CrearVueloRequest request);

        Task<VueloResponse?> ActualizarAsync(
            int id,
            ActualizarVueloRequest request);

        Task<bool> EliminarAsync(int id);
    }
}