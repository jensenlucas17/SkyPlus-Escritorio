using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkyPlus.Desktop.Models;
using SkyPlus.Desktop.Services;

namespace SkyPlus.Desktop
{
    public partial class frmAeronaveABM : Form
    {
        private readonly AeronaveClient _aeronaveClient;
        private readonly AeronaveResponse? _aeronave;

        public frmAeronaveABM(
            AeronaveClient aeronaveClient,
            AeronaveResponse? aeronave)
        {
            InitializeComponent();

            _aeronaveClient = aeronaveClient;
            _aeronave = aeronave;
        }

        private void frmAeronaveABM_Load(
            object sender,
            EventArgs e)
        {
            if (_aeronave != null)
            {
                txtMatricula.Text =
                    _aeronave.Matricula;

                txtModelo.Text =
                    _aeronave.Modelo;
            }
        }

        private async void btnGuardar_Click(
            object sender,
            EventArgs e)
        {
            var matricula =
                txtMatricula.Text.Trim();

            var modelo =
                txtModelo.Text.Trim();

            if (string.IsNullOrWhiteSpace(matricula))
            {
                MessageBox.Show(
                    "Ingrese la matrícula.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtMatricula.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(modelo))
            {
                MessageBox.Show(
                    "Ingrese el modelo.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtModelo.Focus();
                return;
            }

            try
            {
                if (_aeronave == null)
                {
                    await _aeronaveClient.CrearAsync(
                        matricula,
                        modelo);

                    MessageBox.Show(
                        "Aeronave creada correctamente.",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    await _aeronaveClient.ActualizarAsync(
                        _aeronave.IdAeronave,
                        matricula,
                        modelo);

                    MessageBox.Show(
                        "Aeronave actualizada correctamente.",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "No se pudo conectar con la API.\n\n" +
                    ex.Message,
                    "Error de conexión",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocurrió un error al guardar la aeronave.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnCancelar_Click(
            object sender,
            EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}