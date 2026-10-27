namespace SkyPlus.API.DTOs.Reportes
{
    public class PasajeroPorVueloResponse
    {
        public int IdVuelo { get; set; }
        public string NumeroVuelo { get; set; } = string.Empty;
        public DateTime Salida { get; set; }

        public int TotalReservas { get; set; }
        public int ReservasConfirmadas { get; set; }
        public int PasajerosUnicos { get; set; }
    }

    public class PasajeroFrecuenteResponse
    {
        public int IdPasajero { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;

        public int TotalReservas { get; set; }
        public int ReservasConfirmadas { get; set; }
    }

    public class ReportePasajerosResponse
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }

        public int TotalPasajerosRegistrados { get; set; }
        public int PasajerosActivos { get; set; }
        public int PasajerosInactivos { get; set; }

        public int PasajerosConReservas { get; set; }
        public int PasajerosConReservasConfirmadas { get; set; }
        public int TotalReservasPeriodo { get; set; }

        public List<PasajeroPorVueloResponse> PasajerosPorVuelo { get; set; }
            = new();

        public List<PasajeroFrecuenteResponse> PasajerosFrecuentes { get; set; }
            = new();
    }
}
