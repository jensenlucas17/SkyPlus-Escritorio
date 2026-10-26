
using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface IEquipajeService
    {
        Task<List<TipoEquipajeResponse>> ObtenerTiposAsync();

        Task<List<EquipajeReservaResponse>> ObtenerPorReservaAsync(
            int idReserva);

        Task<EquipajeReservaResponse> ContratarAsync(
            ContratarEquipajeRequest request);

        Task<ResultadoPesoEquipajeResponse> RegistrarPesoAsync(
            RegistrarPesoEquipajeRequest request);
    }
}
