namespace SkyPlus.Desktop
{
    partial class frmCheckin
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
            pnlFiltros = new Panel();
            btnBuscar = new Button();
            btnRefrescar = new Button();
            txtBuscar = new TextBox();
            pnlBotones = new Panel();
            btnRealizarCheckin = new Button();
            dgvCheckin = new DataGridView();
            lblEstado = new Label();
            pnlFiltros.SuspendLayout();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCheckin).BeginInit();
            SuspendLayout();
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(lblEstado);
            pnlFiltros.Controls.Add(btnBuscar);
            pnlFiltros.Controls.Add(btnRefrescar);
            pnlFiltros.Controls.Add(txtBuscar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 129);
            pnlFiltros.TabIndex = 0;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(440, 39);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(75, 23);
            btnBuscar.TabIndex = 8;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // btnRefrescar
            // 
            btnRefrescar.Location = new Point(543, 39);
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(75, 23);
            btnRefrescar.TabIndex = 9;
            btnRefrescar.Text = "Refrescar";
            btnRefrescar.UseVisualStyleBackColor = true;
            btnRefrescar.Click += btnRefrescar_Click;
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(182, 39);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(167, 23);
            txtBuscar.TabIndex = 7;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(btnRealizarCheckin);
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Location = new Point(0, 350);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(800, 100);
            pnlBotones.TabIndex = 1;
            // 
            // btnRealizarCheckin
            // 
            btnRealizarCheckin.Location = new Point(336, 21);
            btnRealizarCheckin.Name = "btnRealizarCheckin";
            btnRealizarCheckin.Size = new Size(108, 42);
            btnRealizarCheckin.TabIndex = 0;
            btnRealizarCheckin.Text = "Realizar Checkin";
            btnRealizarCheckin.UseVisualStyleBackColor = true;
            btnRealizarCheckin.Click += btnRealizarCheckin_Click;
            // 
            // dgvCheckin
            // 
            dgvCheckin.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCheckin.Dock = DockStyle.Fill;
            dgvCheckin.Location = new Point(0, 129);
            dgvCheckin.Name = "dgvCheckin";
            dgvCheckin.ReadOnly = true;
            dgvCheckin.Size = new Size(800, 221);
            dgvCheckin.TabIndex = 8;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Location = new Point(182, 99);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(38, 15);
            lblEstado.TabIndex = 10;
            lblEstado.Text = "label1";
            // 
            // frmCheckin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvCheckin);
            Controls.Add(pnlBotones);
            Controls.Add(pnlFiltros);
            Name = "frmCheckin";
            Text = "Form1";
            Load += frmCheckin_Load;
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCheckin).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlFiltros;
        private Button btnBuscar;
        private Button btnRefrescar;
        private TextBox txtBuscar;
        private Panel pnlBotones;
        private Button btnRealizarCheckin;
        private DataGridView dgvCheckin;
        private Label lblEstado;
    }
}