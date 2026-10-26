namespace SkyPlus.API.DTOs
{
    public class RegistrarEmbarqueRequest
    {
        public string CodigoQr { get; set; } = string.Empty;

        public int IdUsuarioAgente { get; set; }
    }
}
