using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IUsuarioService
    {
        Task<List<UsuarioResponse>> ObtenerTodosAsync();
        Task<UsuarioResponse?> ObtenerPorIdAsync(int id);
        Task<UsuarioResponse?> CrearAsync(CrearUsuarioRequest request);
        Task<UsuarioResponse?> ActualizarAsync(int id, ActualizarUsuarioRequest request);
        Task<bool> EliminarAsync(int id);
    }
}