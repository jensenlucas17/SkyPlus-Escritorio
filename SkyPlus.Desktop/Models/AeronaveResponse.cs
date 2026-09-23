// SkyPlus.Desktop/Models/AeronaveResponse.cs
// TODO: reemplazar por el modelo real de Lucas cuando exista.

namespace SkyPlus.Desktop.Models
{
    public class AeronaveResponse
    {
        public int IdAeronave { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;

        public string MatriculaModelo => $"{Matricula} - {Modelo}";
    }
}