namespace SkyPlus.API.DTOs.Reportes
{
    public class CheckInsPorVueloResponse
    {
        public int IdVuelo { get; set; }
        public string NumeroVuelo { get; set; } = string.Empty;
        public DateTime Salida { get; set; }

        public int ReservasConfirmadas { get; set; }
        public int TotalCheckIns { get; set; }
        public int Embarcados { get; set; }
        public int PendientesEmbarque { get; set; }

        public decimal PorcentajeEmbarque { get; set; }
    }

    public class CheckInsPorDiaResponse
    {
        public DateTime Fecha { get; set; }
        public int CantidadCheckIns { get; set; }
    }

    public class ReporteCheckInsResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int TotalCheckIns { get; set; }
        public int Embarcados { get; set; }
        public int PendientesEmbarque { get; set; }

        public decimal PorcentajeEmbarque { get; set; }

        public List<CheckInsPorVueloResponse> CheckInsPorVuelo { get; set; }
            = new();

        public List<CheckInsPorDiaResponse> CheckInsPorDia { get; set; }
            = new();
    }
}