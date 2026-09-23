// SkyPlus.Desktop/Models/VueloResponse.cs
// TODO: reemplazar por el modelo real de Lucas cuando exista.

using System;

namespace SkyPlus.Desktop.Models
{
    public class VueloResponse
    {
        public int IdVuelo { get; set; }
        public string NumeroVuelo { get; set; } = string.Empty;

        public int IdAeronave { get; set; }
        public string Aeronave { get; set; } = string.Empty; // texto ya armado, ej "LV-ABC - Boeing 737-800"

        public int IdLugarOrigen { get; set; }
        public string LugarOrigen { get; set; } = string.Empty;

        public int IdLugarDestino { get; set; }
        public string LugarDestino { get; set; } = string.Empty;

        public DateTime Salida { get; set; }
        public DateTime Llegada { get; set; }

        // TODO: confirmar con Lucas los valores reales de estado_vuelo
        public string EstadoVuelo { get; set; } = "Programado";

        public decimal Tarifa { get; set; }

        public int IdUsuarioOperador { get; set; }

        public string DescripcionReserva =>
            $"{NumeroVuelo} | {Salida:dd/MM/yyyy HH:mm} → {Llegada:HH:mm}";
    }
}