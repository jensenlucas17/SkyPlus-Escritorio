
using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IEquipajeService
    {
        // Obtener los tipos de equipaje disponibles.
        Task<List<TipoEquipajeResponse>> ObtenerTiposAsync();

        // Consultar los equipajes de una reserva.
        Task<List<EquipajeReservaResponse>> ObtenerPorReservaAsync(
            int idReserva);

        // Contratar equipaje durante o después de la reserva.
        Task<EquipajeReservaResponse> ContratarAsync(
            ContratarEquipajeRequest request);
    }
}
