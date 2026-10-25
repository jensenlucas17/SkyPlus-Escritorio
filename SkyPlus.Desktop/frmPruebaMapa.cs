using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;

namespace SkyPlus.Desktop
{
    public partial class frmPruebaMapa : Form
    {
        public frmPruebaMapa()
        {
            InitializeComponent();

            var mapa = new ucMapaAsientos { Dock = DockStyle.Fill };
            Controls.Add(mapa);

            var rnd = new Random(1);
            var asientos = new List<AsientoResponse>();
            foreach (var fila in System.Linq.Enumerable.Range(1, 30))
            {
                foreach (var letra in new[] { "A", "B", "C", "D", "E", "F" })
                {
                    asientos.Add(new AsientoResponse
                    {
                        IdAsiento = asientos.Count + 1,
                        IdVuelo = 1,
                        Fila = fila,
                        Letra = letra,
                        Estado = rnd.Next(100) < 25 ? "Ocupado" : "Libre"
                    });
                }
            }

            mapa.Cargar(asientos);
            mapa.AsientoSeleccionado += (s, a) => Text = $"Asiento elegido: {a.CodigoAsiento}";
        }
    }
}