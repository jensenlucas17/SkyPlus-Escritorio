// SkyPlus.Desktop/frmPasajeros.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;
using SkyPlus.Desktop.Estilos;

namespace SkyPlus.Desktop
{
    public partial class frmPasajeros : Form
    {
        private readonly PasajeroClient _pasajeroClient = new PasajeroClient();
        private List<PasajeroResponse> _pasajeros = new List<PasajeroResponse>();

        public frmPasajeros()
        {
            InitializeComponent();
        }

        private ComboBox _cmbEstado = null!;
        private ComboBox _cmbNacionalidad = null!;
        private Label _lblResumen = null!;

        private async void frmPasajeros_Load(object sender, EventArgs e)
        {
            AppEstilos.EstilizarGrid(dgvPasajeros);
            AppEstilos.EstilizarBotonPrimario(btnNuevo);
            AppEstilos.EstilizarBotonSecundario(btnEditar);
            AppEstilos.EstilizarBotonSecundario(btnEliminar);
            AppEstilos.EstilizarBotonSecundario(btnReactivar);

            // Filtros (el botón del diseñador "btnBuscar" pasa a ser Refrescar)
            _cmbEstado = Listados.Combo(130, new[] { "Activo", "Inactivo" }, AplicarFiltro);
            _cmbNacionalidad = Listados.Combo(160, Array.Empty<string>(), AplicarFiltro);
            Listados.ArmarFiltros(pnlFiltros, txtBuscar, btnBuscar,
                "Nombre, apellido, DNI o email",
                ("Estado", _cmbEstado),
                ("Nacionalidad", _cmbNacionalidad));
            Listados.ConectarBusqueda(txtBuscar, btnBuscar, AplicarFiltro, CargarPasajeros, () =>
            {
                _cmbEstado.SelectedIndex = 0;
                _cmbNacionalidad.SelectedIndex = 0;
            });
            _lblResumen = Listados.CrearResumen(this, dgvPasajeros);

            await CargarPasajeros();
        }

        private void AplicarFiltro()
        {
            if (_lblResumen == null) return;

            var texto = txtBuscar.Text.Trim();
            var lista = _pasajeros
                .Where(p => AppEstilos.Coincide(texto, p.Nombre, p.Apellido, p.Documento, p.Email))
                .Where(p => Listados.Pasa(_cmbEstado, p.Estado))
                .Where(p => Listados.Pasa(_cmbNacionalidad, p.Nacionalidad))
                .OrderBy(p => p.Apellido).ThenBy(p => p.Nombre)
                .ToList();

            dgvPasajeros.DataSource = null;
            dgvPasajeros.DataSource = lista;

            _lblResumen.Text =
                $"{lista.Count} pasajeros   |   {lista.Count(p => p.Estado == "Activo")} activos   |   " +
                $"{lista.Count(p => p.Estado != "Activo")} inactivos   |   " +
                $"{lista.Select(p => p.Nacionalidad).Distinct().Count()} nacionalidades";
        }

        private async Task CargarPasajeros()
        {
            try
            {
                var pasajeros = await _pasajeroClient.ObtenerTodosAsync();
                _pasajeros = pasajeros ?? new List<PasajeroResponse>();
                Listados.CargarOpciones(_cmbNacionalidad, _pasajeros.Select(p => p.Nacionalidad).OrderBy(n => n));
                AplicarFiltro();
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("No se pudo conectar con la API de SkyPlus.",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al cargar los pasajeros.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Este botón ahora es "Refrescar" (lo maneja Listados.ConectarBusqueda)
        private void btnBuscar_Click(object sender, EventArgs e)
        {
        }

        private async void btnNuevo_Click(object sender, EventArgs e)
        {
            using (var formulario = new frmPasajeroABM(_pasajeroClient, null))
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    await CargarPasajeros();
                }
            }
        }

        private async void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvPasajeros.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un pasajero.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var pasajero = dgvPasajeros.CurrentRow.DataBoundItem as PasajeroResponse;
            if (pasajero == null) return;

            using (var formulario = new frmPasajeroABM(_pasajeroClient, pasajero))
            {
                if (formulario.ShowDialog() == DialogResult.OK)
                {
                    await CargarPasajeros();
                }
            }
        }

        private async void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvPasajeros.CurrentRow?.DataBoundItem
                is not PasajeroResponse pasajero)
            {
                MessageBox.Show(
                    "Seleccione un pasajero.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (pasajero.Estado == "Inactivo")
            {
                MessageBox.Show(
                    "Este pasajero ya se encuentra inactivo.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var respuesta = MessageBox.Show(
                $"¿Desea dar de baja a {pasajero.NombreCompleto}?\n\n" +
                "El pasajero quedará inactivo, pero conservará " +
                "sus datos y su historial de reservas.",
                "Confirmar baja",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                await _pasajeroClient.EliminarAsync(pasajero.IdPasajero);

                MessageBox.Show(
                    "Pasajero dado de baja correctamente.",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await CargarPasajeros();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "No se pudo completar la baja del pasajero.\n\n" +
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

        private async void btnReactivar_Click(object sender, EventArgs e)
        {
            if (dgvPasajeros.CurrentRow?.DataBoundItem
                is not PasajeroResponse pasajero)
            {
                MessageBox.Show(
                    "Seleccione un pasajero.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (pasajero.Estado == "Activo")
            {
                MessageBox.Show(
                    "Este pasajero ya se encuentra activo.",
                    "Aviso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var respuesta = MessageBox.Show(
                $"¿Desea reactivar a {pasajero.NombreCompleto}?",
                "Confirmar reactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            try
            {
                await _pasajeroClient.ReactivarAsync(pasajero.IdPasajero);

                MessageBox.Show(
                    "Pasajero reactivado correctamente.",
                    "Éxito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                await CargarPasajeros();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "No se pudo reactivar el pasajero.\n\n" +
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