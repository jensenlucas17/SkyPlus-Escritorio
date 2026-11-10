namespace SkyPlus.Desktop.Models
{
    public class RutaRankingResponse
    {
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;

        public int CantidadVuelos { get; set; }
        public int ReservasConfirmadas { get; set; }

        public int TotalAsientos { get; set; }
        public int AsientosOcupados { get; set; }

        public decimal PorcentajeOcupacion { get; set; }
    }
}