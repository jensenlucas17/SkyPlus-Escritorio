using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmAeronaves : Form
    {
        private readonly AeronaveClient _aeronaveClient =
            new AeronaveClient();

        private List<AeronaveResponse> _aeronaves =
            new List<AeronaveResponse>();

        public frmAeronaves()
        {
            InitializeComponent();
        }

        private async void frmAeronaves_Load(
            object sender,
            EventArgs e)
        {
            AppEstilos.EstilizarGrid(dgvAeronaves);
            AppEstilos.EstilizarBotonPrimario(btnNuevo);
            AppEstilos.EstilizarBotonSecundario(btnEditar);
            AppEstilos.EstilizarBotonSecundario(btnEliminar);
            AppEstilos.EstilizarBotonSecundario(btnReactivar);

            // Filtros (el botón del diseñador "btnBuscar" pasa a ser Refrescar)
            _cmbEstado = Listados.Combo(130, new[] { "Activo", "Inactivo" }, AplicarFiltro);
            _cmbModelo = Listados.Combo(190, Array.Empty<string>(), AplicarFiltro);
            Listados.ArmarFiltros(pnlFiltros, txtBuscar, btnBuscar,
                "Matrícula o modelo",
                ("Estado", _cmbEstado),
                ("Modelo", _cmbModelo));
            Listados.ConectarBusqueda(txtBuscar, btnBuscar, AplicarFiltro, CargarAeronaves, () =>
            {
                _cmbEstado.SelectedIndex = 0;
                _cmbModelo.SelectedIndex = 0;
            });
            _lblResumen = Listados.CrearResumen(this, dgvAeronaves);

            await CargarAeronaves();
        }

        private ComboBox _cmbEstado = null!;
        private ComboBox _cmbModelo = null!;
        private Label _lblResumen = null!;

        private void AplicarFiltro()
        {
            if (_lblResumen == null) return;

            var texto = txtBuscar.Text.Trim();
            var lista = _aeronaves
                .Where(a => AppEstilos.Coincide(texto, a.Matricula, a.Modelo))
                .Where(a => Listados.Pasa(_cmbEstado, a.Estado))
                .Where(a => Listados.Pasa(_cmbModelo, a.Modelo))
                .ToList();

            dgvAeronaves.DataSource = null;
            dgvAeronaves.DataSource = lista;

            _lblResumen.Text =
                $"{lista.Count} aeronaves   |   {lista.Count(a => a.Estado == "Activo")} activas   |   " +
                $"{lista.Count(a => a.Estado != "Activo")} inactivas   |   " +
                "Cada aeronave tiene 180 asientos (30 filas × 6)";
        }

        private async Task CargarAeronaves()
        {
            try
            {
                var aeronaves =
                    await _aeronaveClient.ObtenerTodosAsync();

                _aeronaves = aeronaves ??
                    new List<AeronaveResponse>();

                Listados.CargarOpciones(_cmbModelo, _aeronaves.Select(a => a.Modelo).OrderBy(m => m));
                AplicarFiltro();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "No se pudo conectar con la API de SkyPlus.",
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al cargar las aeronaves.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void btnNuevo_Click(
            object sender,
            EventArgs e)
        {
            using (var formulario =
                new frmAeronaveABM(
                    _aeronaveClient,
                    null))
            {
                if (formulario.ShowDialog() ==
                    DialogResult.OK)
                {
                    await CargarAeronaves();
                }
            }
        }

        private async void btnEditar_Click(
            object sender,
            EventArgs e)
        {
            if (dgvAeronaves.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione una aeronave.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            var aeronave =
                dgvAeronaves.CurrentRow.DataBoundItem
                as AeronaveResponse;

            if (aeronave == null)
                return;

            using (var formulario =
                new frmAeronaveABM(
                    _aeronaveClient,
                    aeronave))
            {
                if (formulario.ShowDialog() ==
                    DialogResult.OK)
                {
                    await CargarAeronaves();
                }
            }
        }

        private async void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvAeronaves.CurrentRow?.DataBoundItem
                is not AeronaveResponse aeronave)
            {
                MessageBox.Show(
                    "Seleccione una aeronave.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (aeronave.Estado == "Inactivo")
            {
                MessageBox.Show(
                    "Esta aeronave ya se encuentra inactiva.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var respuesta = MessageBox.Show(
                $"¿Desea dar de baja la aeronave {aeronave.Matricula}?\n\n" +
                "La aeronave quedará inactiva, pero conservará su historial.",
                "Confirmar baja",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                await _aeronaveClient.EliminarAsync(aeronave.IdAeronave);

                MessageBox.Show(
                    "Aeronave dada de baja correctamente.",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await CargarAeronaves();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "No se pudo completar la baja de la aeronave.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Este botón ahora es "Refrescar" (lo maneja Listados.ConectarBusqueda)
        private void btnActualizar_Click(
            object sender,
            EventArgs e)
        {
        }

        private async void btnReactivar_Click(object sender, EventArgs e)
        {
            if (dgvAeronaves.CurrentRow?.DataBoundItem
                is not AeronaveResponse aeronave)
            {
                MessageBox.Show(
                    "Seleccione una aeronave.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (aeronave.Estado == "Activo")
            {
                MessageBox.Show(
                    "Esta aeronave ya se encuentra activa.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var respuesta = MessageBox.Show(
                $"¿Desea reactivar la aeronave {aeronave.Matricula}?",
                "Confirmar reactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                await _aeronaveClient.ReactivarAsync(aeronave.IdAeronave);

                MessageBox.Show(
                    "Aeronave reactivada correctamente.",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await CargarAeronaves();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "No se pudo reactivar la aeronave.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}