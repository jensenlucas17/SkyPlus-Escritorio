namespace SkyPlus.API.DTOs.Reportes
{
    public class ReservaDiariaResponse
    {
        public DateTime Fecha { get; set; }
        public int CantidadReservas { get; set; }
    }

    public class ReservaPorEstadoResponse
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class ReporteReservasResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int TotalReservas { get; set; }
        public int Confirmadas { get; set; }
        public int PendientesPago { get; set; }
        public int Canceladas { get; set; }
        public int Expiradas { get; set; }

        public List<ReservaPorEstadoResponse> ReservasPorEstado { get; set; }
            = new();

        public List<ReservaDiariaResponse> ReservasPorDia { get; set; }
            = new();
    }
}
