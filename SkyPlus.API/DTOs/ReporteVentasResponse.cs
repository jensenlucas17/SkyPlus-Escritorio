
namespace SkyPlus.API.DTOs.Reportes
{
    public class VentaDiariaResponse
    {
        public DateTime Fecha { get; set; }
        public int CantidadPagos { get; set; }
        public decimal TotalCobrado { get; set; }
        public decimal TotalReembolsado { get; set; }
        public decimal IngresoNeto { get; set; }
    }

    public class ReporteVentasResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int CantidadPagos { get; set; }
        public int PagosIniciales { get; set; }
        public int PagosAdicionales { get; set; }

        public decimal TotalCobrado { get; set; }
        public decimal TotalReembolsado { get; set; }
        public decimal IngresoNeto { get; set; }

        public List<VentaDiariaResponse> VentasPorDia { get; set; }
            = new();
    }
}
