namespace SkyPlus.Desktop.Models
{
    public class CheckInResponse
    {
        public int IdCheckin { get; set; }
        public int IdReserva { get; set; }
        public string CodigoReserva { get; set; } = string.Empty;
        public int IdUsuarioAgente { get; set; }
        public string CodigoQr { get; set; } = string.Empty;
        public string BoardingPass { get; set; } = string.Empty;
        public DateTime FechaCheckin { get; set; }
        public string EstadoCheckin { get; set; } = string.Empty;

        // La API guarda la fecha en UTC; esto la muestra en hora local
        public DateTime FechaLocal => FechaCheckin.ToLocalTime();
    }

    public class EstadoCheckInResponse
    {
        public int IdReserva { get; set; }
        public bool PuedeRealizarCheckIn { get; set; }
        public int EquipajesSinPesar { get; set; }
        public int CargosPendientes { get; set; }
        public decimal MontoPendiente { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public class ValidarQrResponse
    {
        public bool EsValido { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        public int? IdCheckin { get; set; }
        public int? IdReserva { get; set; }

        public string? CodigoReserva { get; set; }
        public string? Pasajero { get; set; }
        public string? NumeroVuelo { get; set; }
        public string? Asiento { get; set; }

        public string? EstadoCheckin { get; set; }
    }
}