namespace SkyPlus.API.DTOs
{
    public class ActualizarVueloRequest
    {
        public string NumeroVuelo { get; set; } = string.Empty;

        public int IdAeronave { get; set; }

        public int IdUsuarioOperador { get; set; }

        public int IdLugarOrigen { get; set; }

        public int IdLugarDestino { get; set; }

        public DateTime Salida { get; set; }

        public DateTime Llegada { get; set; }

        public string EstadoVuelo { get; set; } = "Programado";

        public decimal Tarifa { get; set; }
    }
}