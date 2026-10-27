namespace SkyPlus.API.DTOs
{
    public class CrearPagoRequest
    {
        public int IdReserva { get; set; }

        public string MetodoPago { get; set; } = string.Empty;
    }
}