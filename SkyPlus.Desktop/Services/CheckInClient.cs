using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class CheckInClient
    {
        private readonly ApiClient _apiClient;

        public CheckInClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<CheckInResponse>?> ObtenerTodosAsync()
        {
            return await _apiClient.GetAsync<List<CheckInResponse>>(
                "api/CheckIns");
        }

        public async Task<EstadoCheckInResponse?> ConsultarEstadoAsync(int idReserva)
        {
            return await _apiClient.GetAsync<EstadoCheckInResponse>(
                $"api/CheckIns/estado/{idReserva}");
        }

        public async Task<CheckInResponse?> CrearAsync(int idReserva)
        {
            return await _apiClient.PostAsync<object, CheckInResponse>(
                "api/CheckIns",
                new { IdReserva = idReserva });
        }

        public async Task<byte[]> DescargarBoardingPassAsync(int idCheckin)
        {
            return await _apiClient.GetBytesAsync(
                $"api/CheckIns/{idCheckin}/boarding-pass");
        }

        public async Task<ValidarQrResponse?> RegistrarEmbarqueAsync(string codigoQr)
        {
            return await _apiClient.PostAsync<object, ValidarQrResponse>(
                "api/CheckIns/embarcar",
                new { CodigoQr = codigoQr });
        }
    }
}