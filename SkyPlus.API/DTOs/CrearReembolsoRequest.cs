namespace SkyPlus.API.DTOs
{
    public class CrearReembolsoRequest
    {
        public int IdPago { get; set; }

        public string Motivo { get; set; } = string.Empty;
    }
}