using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface ILugarService
    {
        Task<List<LugarResponse>> ObtenerTodosAsync();

        Task<LugarResponse?> ObtenerPorIdAsync(int id);

        Task<LugarResponse?> CrearAsync(CrearLugarRequest request);

        Task<LugarResponse?> ActualizarAsync(
            int id,
            ActualizarLugarRequest request);

        Task<bool> EliminarAsync(int id);
    }
}