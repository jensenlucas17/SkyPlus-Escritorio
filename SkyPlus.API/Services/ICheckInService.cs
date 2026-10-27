
using SkyPlus.API.DTOs;

namespace SkyPlus.API.Services
{
    public interface ICheckInService
    {
        Task<List<CheckInResponse>> ObtenerTodosAsync();

        Task<CheckInResponse?> ObtenerPorIdAsync(int id);

        Task<EstadoCheckInResponse> ConsultarEstadoAsync(
            int idReserva);

        Task<CheckInResponse> CrearAsync(
            CrearCheckInRequest request,
            int idUsuarioAutenticado);

        Task<ValidarQrResponse> ValidarQrAsync(string codigoQr);
        
        Task<ValidarQrResponse> RegistrarEmbarqueAsync(
            RegistrarEmbarqueRequest request,
            int idUsuarioAutenticado);
    }
}
