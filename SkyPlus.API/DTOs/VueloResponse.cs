namespace SkyPlus.API.DTOs
{
    public class VueloResponse
    {
        public int IdVuelo { get; set; }

        public string NumeroVuelo { get; set; } = string.Empty;

        public int IdAeronave { get; set; }

        public int IdUsuarioOperador { get; set; }

        public int IdLugarOrigen { get; set; }

        public int IdLugarDestino { get; set; }

        public DateTime Salida { get; set; }

        public DateTime Llegada { get; set; }

        public string EstadoVuelo { get; set; } = string.Empty;

        public decimal Tarifa { get; set; }

        public string Aeronave { get; set; } = string.Empty;

        public string LugarOrigen { get; set; } = string.Empty;

        public string LugarDestino { get; set; } = string.Empty;
    }
}