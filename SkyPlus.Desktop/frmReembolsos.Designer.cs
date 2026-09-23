namespace SkyPlus.Desktop
{
    partial class frmReembolsos
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
            lblTitulo = new Label();
            lblBuscar = new Label();
            txtBuscar = new TextBox();
            btnBuscar = new Button();
            btnRefrescar = new Button();
            btnVerDetalle = new Button();
            lblCantidad = new Label();
            dgvReembolsos = new DataGridView();
            pnlFiltros = new Panel();
            pnlBotones = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvReembolsos).BeginInit();
            pnlFiltros.SuspendLayout();
            pnlBotones.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(322, 7);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(80, 15);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "REEMBOLSOS";
            // 
            // lblBuscar
            // 
            lblBuscar.AutoSize = true;
            lblBuscar.Location = new Point(99, 54);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(45, 15);
            lblBuscar.TabIndex = 1;
            lblBuscar.Text = "Buscar:";
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(183, 51);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(239, 23);
            txtBuscar.TabIndex = 2;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(443, 50);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(75, 23);
            btnBuscar.TabIndex = 3;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // btnRefrescar
            // 
            btnRefrescar.Location = new Point(536, 51);
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(75, 23);
            btnRefrescar.TabIndex = 4;
            btnRefrescar.Text = "Refrescar";
            btnRefrescar.UseVisualStyleBackColor = true;
            btnRefrescar.Click += btnRefrescar_Click;
            // 
            // btnVerDetalle
            // 
            btnVerDetalle.Location = new Point(518, 34);
            btnVerDetalle.Name = "btnVerDetalle";
            btnVerDetalle.Size = new Size(75, 23);
            btnVerDetalle.TabIndex = 5;
            btnVerDetalle.Text = "Ver Detalle";
            btnVerDetalle.UseVisualStyleBackColor = true;
            btnVerDetalle.Click += btnVerDetalle_Click;
            // 
            // lblCantidad
            // 
            lblCantidad.AutoSize = true;
            lblCantidad.Location = new Point(183, 38);
            lblCantidad.Name = "lblCantidad";
            lblCantidad.Size = new Size(83, 15);
            lblCantidad.TabIndex = 6;
            lblCantidad.Text = "Reembolsos: 4";
            // 
            // dgvReembolsos
            // 
            dgvReembolsos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvReembolsos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReembolsos.Location = new Point(0, 98);
            dgvReembolsos.Name = "dgvReembolsos";
            dgvReembolsos.Size = new Size(797, 253);
            dgvReembolsos.TabIndex = 7;
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(btnRefrescar);
            pnlFiltros.Controls.Add(lblTitulo);
            pnlFiltros.Controls.Add(lblBuscar);
            pnlFiltros.Controls.Add(txtBuscar);
            pnlFiltros.Controls.Add(btnBuscar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 100);
            pnlFiltros.TabIndex = 8;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(btnVerDetalle);
            pnlBotones.Controls.Add(lblCantidad);
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Location = new Point(0, 350);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(800, 100);
            pnlBotones.TabIndex = 9;
            // 
            // frmReembolsos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pnlBotones);
            Controls.Add(pnlFiltros);
            Controls.Add(dgvReembolsos);
            Name = "frmReembolsos";
            Text = "frmReembolsos";
            Load += frmReembolsos_Load;
            ((System.ComponentModel.ISupportInitialize)dgvReembolsos).EndInit();
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            pnlBotones.ResumeLayout(false);
            pnlBotones.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitulo;
        private Label lblBuscar;
        private TextBox txtBuscar;
        private Button btnBuscar;
        private Button btnRefrescar;
        private Button btnVerDetalle;
        private Label lblCantidad;
        private DataGridView dgvReembolsos;
        private Panel pnlFiltros;
        private Panel pnlBotones;
    }
}