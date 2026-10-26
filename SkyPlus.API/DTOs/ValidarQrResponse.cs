namespace SkyPlus.API.DTOs
{
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
