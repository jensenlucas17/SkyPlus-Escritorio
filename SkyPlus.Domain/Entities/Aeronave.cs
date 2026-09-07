namespace SkyPlus.Domain.Entities
{
    public class Aeronave
    {
        public int IdAeronave { get; set; }

        public string Matricula { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;
    }
}