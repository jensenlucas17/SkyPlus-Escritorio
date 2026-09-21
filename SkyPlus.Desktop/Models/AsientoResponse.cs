namespace SkyPlus.Desktop.Models
{
    public class AsientoResponse
    {
        public int IdAsiento { get; set; }

        public int IdVuelo { get; set; }

        public int Fila { get; set; }

        public string Letra { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string CodigoAsiento =>
            $"{Fila}{Letra}";
    }
}