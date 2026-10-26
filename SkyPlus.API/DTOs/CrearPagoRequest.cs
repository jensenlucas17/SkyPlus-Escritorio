namespace SkyPlus.API.DTOs
{
    public class CrearPagoRequest
    {
        public int IdReserva { get; set; }
        public int IdUsuarioAgente { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
    }
}
