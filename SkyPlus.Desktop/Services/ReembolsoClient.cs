
using SkyPlus.Desktop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkyPlus.Desktop.Services
{
    public class ReembolsoClient
    {
        private readonly ApiClient _api = new ApiClient();

        public async Task<List<ReembolsoResponse>> ObtenerTodosAsync()
        {
            return await _api.GetAsync<List<ReembolsoResponse>>(
                "api/Reembolsos"
            ) ?? new List<ReembolsoResponse>();
        }

        public Task<ReembolsoResponse?> ObtenerPorIdAsync(int id)
        {
            return _api.GetAsync<ReembolsoResponse>(
                $"api/Reembolsos/{id}"
            );
        }

        public Task<ReembolsoResponse?> CrearAsync(
            int idPago,
            int idUsuarioAgente,
            string motivo)
        {
            return _api.PostAsync<object, ReembolsoResponse>(
                "api/Reembolsos",
                new
                {
                    idPago,
                    idUsuarioAgente,
                    motivo
                }
            );
        }
    }
}
