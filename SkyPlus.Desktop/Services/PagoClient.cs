using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop.Services
{
    public class PagoClient
    {
        private readonly ApiClient _api = new ApiClient();

        public async Task<List<PagoResponse>> ObtenerTodosAsync()
            => await _api.GetAsync<List<PagoResponse>>("api/Pagos") ?? new List<PagoResponse>();

        public Task<PagoResponse?> ObtenerPorIdAsync(int id)
            => _api.GetAsync<PagoResponse>($"api/Pagos/{id}");

        public Task<PagoResponse?> CrearAsync(int idReserva, int idUsuarioAgente, string metodoPago)
            => _api.PostAsync<object, PagoResponse>("api/Pagos", new
            {
                idReserva,
                idUsuarioAgente,
                metodoPago
            });

        // Pago de cargos pendientes de una reserva confirmada.
        // Si lo usa el Agente de Check-in, la API cobra solo el exceso de peso.
        public Task<PagoResponse?> CrearAdicionalAsync(int idReserva, string metodoPago)
            => _api.PostAsync<object, PagoResponse>("api/Pagos/adicional", new
            {
                idReserva,
                metodoPago
            });

        public async Task AnularAsync(int id)
        {
            await _api.PutAsync<object, System.Text.Json.JsonElement>(
                $"api/Pagos/{id}/anular", new { });
        }
    }
}
