namespace SkyPlus.Desktop.Models
{
    public class ReporteFiltrosRequest
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int? IdVuelo { get; set; }
        public int? IdLugarOrigen { get; set; }
        public int? IdLugarDestino { get; set; }

        public string? EstadoVuelo { get; set; }
        public string? EstadoReserva { get; set; }
        public string? MetodoPago { get; set; }
        public string? TipoPago { get; set; }

        public string Agrupacion { get; set; } = "Dia";

        public bool CompararPeriodoAnterior { get; set; } = false;
    }
}
