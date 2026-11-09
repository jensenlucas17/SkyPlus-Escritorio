using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class EquipajeClient
    {
        private readonly ApiClient _apiClient;

        public EquipajeClient()
        {
            _apiClient = new ApiClient();
        }

        public async Task<List<TipoEquipajeResponse>?> ObtenerTiposAsync()
        {
            return await _apiClient.GetAsync<List<TipoEquipajeResponse>>(
                "api/Equipajes/tipos");
        }

        public async Task<List<EquipajeReservaResponse>?> ObtenerPorReservaAsync(int idReserva)
        {
            return await _apiClient.GetAsync<List<EquipajeReservaResponse>>(
                $"api/Equipajes/reserva/{idReserva}");
        }

        // Agrega una valija a la reserva. Queda "pendiente de pago" hasta que se cobra.
        public async Task<EquipajeReservaResponse?> ContratarAsync(int idReserva, int idTipoEquipaje)
        {
            return await _apiClient.PostAsync<object, EquipajeReservaResponse>(
                "api/Equipajes/contratar",
                new { IdReserva = idReserva, IdTipoEquipaje = idTipoEquipaje });
        }

        // Solo el Agente de Check-in puede pesar el equipaje
        public async Task<ResultadoPesoEquipajeResponse?> RegistrarPesoAsync(
            int idEquipajeReserva, decimal pesoKg)
        {
            return await _apiClient.PutAsync<object, ResultadoPesoEquipajeResponse>(
                "api/Equipajes/registrar-peso",
                new
                {
                    IdEquipajeReserva = idEquipajeReserva,
                    IdUsuarioAgente = SesionUsuario.IdUsuario,
                    PesoKg = pesoKg
                });
        }
    }
}
