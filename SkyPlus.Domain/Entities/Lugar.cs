namespace SkyPlus.Domain.Entities
{
    public class Lugar
    {
        public int IdLugar { get; set; }

        public string CodigoIata { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Ciudad { get; set; } = string.Empty;

        public string Pais { get; set; } = string.Empty;
    }
}