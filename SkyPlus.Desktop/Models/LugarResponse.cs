using System;
using System.Collections.Generic;
using System.Text;

namespace SkyPlus.Desktop.Models
{
    public class LugarResponse
    {
        public int IdLugar { get; set; }
        public string CodigoIata { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string IataCiudad => $"{CodigoIata} - {Ciudad}";
    }
}
