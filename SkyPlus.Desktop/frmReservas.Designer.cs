namespace SkyPlus.Desktop
{
    partial class frmReservas
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dgvReservas = new DataGridView();
            pnlBotones = new Panel();
            btnCancelarReserva = new Button();
            btnNuevo = new Button();
            pnlFiltros = new Panel();
            btnBuscar = new Button();
            txtBuscar = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvReservas).BeginInit();
            pnlBotones.SuspendLayout();
            pnlFiltros.SuspendLayout();
            SuspendLayout();
            // 
            // dgvReservas
            // 
            dgvReservas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReservas.Dock = DockStyle.Fill;
            dgvReservas.Location = new Point(0, 100);
            dgvReservas.Name = "dgvReservas";
            dgvReservas.ReadOnly = true;
            dgvReservas.Size = new Size(800, 264);
            dgvReservas.TabIndex = 5;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(btnCancelarReserva);
            pnlBotones.Controls.Add(btnNuevo);
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Location = new Point(0, 364);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(800, 86);
            pnlBotones.TabIndex = 4;
            // 
            // btnCancelarReserva
            // 
            btnCancelarReserva.Location = new Point(484, 15);
            btnCancelarReserva.Name = "btnCancelarReserva";
            btnCancelarReserva.Size = new Size(94, 53);
            btnCancelarReserva.TabIndex = 1;
            btnCancelarReserva.Text = "Cancelar Reserva";
            btnCancelarReserva.UseVisualStyleBackColor = true;
            btnCancelarReserva.Click += btnCancelarReserva_Click;
            // 
            // btnNuevo
            // 
            btnNuevo.Location = new Point(275, 30);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(75, 23);
            btnNuevo.TabIndex = 0;
            btnNuevo.Text = "Nuevo";
            btnNuevo.UseVisualStyleBackColor = true;
            btnNuevo.Click += btnNuevo_Click;
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(btnBuscar);
            pnlFiltros.Controls.Add(txtBuscar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 100);
            pnlFiltros.TabIndex = 3;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(484, 34);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(75, 23);
            btnBuscar.TabIndex = 1;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(232, 35);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(153, 23);
            txtBuscar.TabIndex = 0;
            // 
            // frmReservas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvReservas);
            Controls.Add(pnlBotones);
            Controls.Add(pnlFiltros);
            Name = "frmReservas";
            Text = "Form1";
            Load += frmReservas_Load;
            ((System.ComponentModel.ISupportInitialize)dgvReservas).EndInit();
            pnlBotones.ResumeLayout(false);
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvReservas;
        private Panel pnlBotones;
        private Button btnCancelarReserva;
        private Button btnNuevo;
        private Panel pnlFiltros;
        private Button btnBuscar;
        private TextBox txtBuscar;
    }
}