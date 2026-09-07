namespace SkyPlus.Domain.Entities
{
    public class Vuelo
    {
        public int IdVuelo { get; set; }

        public int IdAeronave { get; set; }

        public int IdUsuarioOperador { get; set; }

        public int IdLugarOrigen { get; set; }

        public int IdLugarDestino { get; set; }

        public string NumeroVuelo { get; set; } = string.Empty;

        public DateTime Salida { get; set; }

        public DateTime Llegada { get; set; }

        public string EstadoVuelo { get; set; } = string.Empty;

        public decimal Tarifa { get; set; }

        // Relaciones
        public Aeronave? Aeronave { get; set; }

        public Usuario? UsuarioOperador { get; set; }

        public Lugar? LugarOrigen { get; set; }

        public Lugar? LugarDestino { get; set; }
    }
}