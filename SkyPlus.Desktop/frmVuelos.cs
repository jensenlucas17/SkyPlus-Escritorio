// SkyPlus.Desktop/frmVuelos.cs

using SkyPlus.Desktop.Estilos;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkyPlus.Desktop
{
    public partial class frmVuelos : Form
    {
        private readonly VueloClient _vueloClient =
            new VueloClient();

        private List<VueloResponse> _vuelosCache =
            new List<VueloResponse>();

        public frmVuelos()
        {
            InitializeComponent();
        }

        private async void frmVuelos_Load(
            object sender,
            EventArgs e)
        {
            ConfigurarColumnas();

            cmbEstadoFiltro.Items.Clear();

            cmbEstadoFiltro.Items.AddRange(
                new object[]
                {
                    "Todos",
                    "Programado",
                    "Confirmado",
                    "Cancelado",
                    "Finalizado"
                });

            cmbEstadoFiltro.SelectedIndex = 0;
            cmbEstadoFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstadoFiltro.FlatStyle = FlatStyle.Flat;
            cmbEstadoFiltro.Font = AppEstilos.FuenteNormal;
            cmbEstadoFiltro.Width = 140;
            cmbEstadoFiltro.SelectedIndexChanged += (s, ev) => AplicarFiltros();

            AppEstilos.EstilizarGrid(dgvVuelos);
            AppEstilos.EstilizarBotonPrimario(btnNuevo);
            AppEstilos.EstilizarBotonSecundario(btnEditar);
            AppEstilos.EstilizarBotonSecundario(btnEliminar);

            // Filtros: cada uno con su título para que se entienda qué hace
            _cmbOrigen = Listados.Combo(190, Array.Empty<string>(), AplicarFiltros);
            _cmbDestino = Listados.Combo(190, Array.Empty<string>(), AplicarFiltros);
            _dtpDia = Listados.Fecha(AplicarFiltros);

            Listados.ArmarFiltros(panel1, txtBuscar, btnRefrescar,
                "N° de vuelo / aeronave",
                "Ej.: SP312 o LV-KAY",
                ("Estado del vuelo", cmbEstadoFiltro),
                ("Origen", _cmbOrigen),
                ("Destino", _cmbDestino),
                ("Día de salida", _dtpDia));
            Listados.ConectarBusqueda(txtBuscar, btnRefrescar, AplicarFiltros, CargarVuelos, () =>
            {
                cmbEstadoFiltro.SelectedIndex = 0;
                _cmbOrigen.SelectedIndex = 0;
                _cmbDestino.SelectedIndex = 0;
                _dtpDia.Checked = false;
            });
            _lblResumen = Listados.CrearResumen(this, dgvVuelos);

            await CargarVuelos();
        }

        private ComboBox _cmbOrigen = null!;
        private ComboBox _cmbDestino = null!;
        private DateTimePicker _dtpDia = null!;
        private Label _lblResumen = null!;

        private void ConfigurarColumnas()
        {
            dgvVuelos.AutoGenerateColumns = false;
            dgvVuelos.Columns.Clear();

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.NumeroVuelo),
                    HeaderText = "N° Vuelo",
                    Width = 90
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.LugarOrigen),
                    HeaderText = "Origen",
                    Width = 130
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.LugarDestino),
                    HeaderText = "Destino",
                    Width = 130
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.Aeronave),
                    HeaderText = "Aeronave",
                    Width = 170
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.Salida),
                    HeaderText = "Salida",
                    Width = 120,
                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Format = "dd/MM/yyyy HH:mm"
                        }
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.Llegada),
                    HeaderText = "Llegada",
                    Width = 120,
                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Format = "dd/MM/yyyy HH:mm"
                        }
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.EstadoVuelo),
                    HeaderText = "Estado",
                    Width = 110
                });

            dgvVuelos.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName =
                        nameof(VueloResponse.Tarifa),
                    HeaderText = "Tarifa",
                    Width = 100,
                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Format = "C0"
                        }
                });
        }

        private async Task CargarVuelos()
        {
            try
            {
                var vuelos =
                    await _vueloClient.ObtenerTodosAsync();

                _vuelosCache =
                    vuelos ??
                    new List<VueloResponse>();

                Listados.CargarOpciones(_cmbOrigen, _vuelosCache.Select(v => v.LugarOrigen).OrderBy(x => x));
                Listados.CargarOpciones(_cmbDestino, _vuelosCache.Select(v => v.LugarDestino).OrderBy(x => x));
                AplicarFiltros();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "No se pudo conectar con la API de SkyPlus.\n\n" +
                    "Verificá que SkyPlus.API esté ejecutándose.",
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al cargar los vuelos:\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltros()
        {
            if (_lblResumen == null) return;

            var texto = txtBuscar.Text.Trim();
            var filtrados = _vuelosCache
                .Where(v => AppEstilos.Coincide(texto, v.NumeroVuelo, v.Aeronave))
                .Where(v => Listados.Pasa(cmbEstadoFiltro, v.EstadoVuelo))
                .Where(v => Listados.Pasa(_cmbOrigen, v.LugarOrigen))
                .Where(v => Listados.Pasa(_cmbDestino, v.LugarDestino))
                .Where(v => !_dtpDia.Checked || v.Salida.Date == _dtpDia.Value.Date)
                .OrderBy(v => v.EstadoVuelo == "Finalizado" || v.EstadoVuelo == "Cancelado" ? 1 : 0)
                .ThenBy(v => v.Salida)
                .ToList();

            dgvVuelos.DataSource = null;
            dgvVuelos.DataSource = filtrados;

            _lblResumen.Text =
                $"{filtrados.Count} vuelos   |   {filtrados.Count(v => v.EstadoVuelo == "Programado")} programados   |   " +
                $"{filtrados.Count(v => v.EstadoVuelo == "Confirmado")} confirmados   |   " +
                $"{filtrados.Count(v => v.EstadoVuelo == "Cancelado")} cancelados   |   " +
                $"{filtrados.Count(v => v.EstadoVuelo == "Finalizado")} finalizados";
        }

        private void txtBuscar_TextChanged(
            object sender,
            EventArgs e)
        {
            AplicarFiltros();
        }

        private void cmbEstadoFiltro_SelectedIndexChanged(
            object sender,
            EventArgs e)
        {
            AplicarFiltros();
        }

        // Botón Refrescar (lo maneja Listados.ConectarBusqueda)
        private void btnRefrescar_Click(
            object sender,
            EventArgs e)
        {
        }

        private VueloResponse? ObtenerSeleccionado()
        {
            if (dgvVuelos.CurrentRow?.DataBoundItem
                is VueloResponse vuelo)
            {
                return vuelo;
            }

            MessageBox.Show(
                "Seleccioná un vuelo de la lista primero.",
                "Atención",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return null;
        }

        private async void btnNuevo_Click(
            object sender,
            EventArgs e)
        {
            using var form =
                new frmVueloABM(
                    _vueloClient,
                    null);

            if (form.ShowDialog() ==
                DialogResult.OK)
            {
                await CargarVuelos();
            }
        }

        private async void btnEditar_Click(
            object sender,
            EventArgs e)
        {
            var seleccionado =
                ObtenerSeleccionado();

            if (seleccionado == null)
                return;

            using var form =
                new frmVueloABM(
                    _vueloClient,
                    seleccionado);

            if (form.ShowDialog() ==
                DialogResult.OK)
            {
                await CargarVuelos();
            }
        }

        private async void btnEliminar_Click(object sender, EventArgs e)
        {
            var vuelo = ObtenerSeleccionado();
            if (vuelo == null) return;

            if (vuelo.EstadoVuelo == "Cancelado")
            {
                MessageBox.Show("Este vuelo ya está cancelado.", "Vuelos",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (vuelo.EstadoVuelo == "En vuelo" || vuelo.EstadoVuelo == "Finalizado")
            {
                MessageBox.Show("No se puede cancelar un vuelo en vuelo o finalizado.", "Vuelos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Confirmás cancelar el vuelo {vuelo.NumeroVuelo}? Las reservas asociadas se verán afectadas.",
                "Cancelar vuelo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                await _vueloClient.ActualizarAsync(
                    vuelo.IdVuelo,
                    vuelo.NumeroVuelo,
                    vuelo.IdAeronave,
                    vuelo.IdUsuarioOperador,
                    vuelo.IdLugarOrigen,
                    vuelo.IdLugarDestino,
                    vuelo.Salida,
                    vuelo.Llegada,
                    "Cancelado",
                    vuelo.Tarifa);

                MessageBox.Show("Vuelo cancelado correctamente.", "Vuelos",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await CargarVuelos();
            }
            catch (HttpRequestException ex)
            {
                MensajesApi.MostrarError(ex);
            }
        }
    }
    
 }
