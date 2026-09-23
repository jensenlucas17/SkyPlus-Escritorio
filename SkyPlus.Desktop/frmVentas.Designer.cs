namespace SkyPlus.Desktop
{
    partial class frmVentas
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
            dgvVentas = new DataGridView();
            pnlFiltros = new Panel();
            pnlBotones = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvVentas).BeginInit();
            pnlFiltros.SuspendLayout();
            pnlBotones.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(390, 1);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(49, 15);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "VENTAS";
            // 
            // lblBuscar
            // 
            lblBuscar.AutoSize = true;
            lblBuscar.Location = new Point(146, 68);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.Size = new Size(45, 15);
            lblBuscar.TabIndex = 1;
            lblBuscar.Text = "Buscar:";
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(197, 65);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(187, 23);
            txtBuscar.TabIndex = 2;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(419, 65);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(75, 23);
            btnBuscar.TabIndex = 3;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // btnRefrescar
            // 
            btnRefrescar.Location = new Point(522, 65);
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(75, 23);
            btnRefrescar.TabIndex = 4;
            btnRefrescar.Text = "Refrescar";
            btnRefrescar.UseVisualStyleBackColor = true;
            btnRefrescar.Click += btnRefrescar_Click;
            // 
            // btnVerDetalle
            // 
            btnVerDetalle.Location = new Point(419, 7);
            btnVerDetalle.Name = "btnVerDetalle";
            btnVerDetalle.Size = new Size(75, 23);
            btnVerDetalle.TabIndex = 5;
            btnVerDetalle.Text = "Ver detalle";
            btnVerDetalle.UseVisualStyleBackColor = true;
            btnVerDetalle.Click += btnVerDetalle_Click;
            // 
            // lblCantidad
            // 
            lblCantidad.AutoSize = true;
            lblCantidad.Location = new Point(146, 11);
            lblCantidad.Name = "lblCantidad";
            lblCantidad.Size = new Size(53, 15);
            lblCantidad.TabIndex = 6;
            lblCantidad.Text = "Ventas: 4";
            // 
            // dgvVentas
            // 
            dgvVentas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvVentas.Dock = DockStyle.Fill;
            dgvVentas.Location = new Point(0, 0);
            dgvVentas.Name = "dgvVentas";
            dgvVentas.ReadOnly = true;
            dgvVentas.Size = new Size(800, 450);
            dgvVentas.TabIndex = 7;
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(btnBuscar);
            pnlFiltros.Controls.Add(lblTitulo);
            pnlFiltros.Controls.Add(lblBuscar);
            pnlFiltros.Controls.Add(txtBuscar);
            pnlFiltros.Controls.Add(btnRefrescar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 100);
            pnlFiltros.TabIndex = 8;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(lblCantidad);
            pnlBotones.Controls.Add(btnVerDetalle);
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Location = new Point(0, 350);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(800, 100);
            pnlBotones.TabIndex = 9;
            // 
            // frmVentas
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pnlBotones);
            Controls.Add(pnlFiltros);
            Controls.Add(dgvVentas);
            Name = "frmVentas";
            Text = "frmVentas";
            Load += frmVentas_Load;
            ((System.ComponentModel.ISupportInitialize)dgvVentas).EndInit();
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
        private DataGridView dgvVentas;
        private Panel pnlFiltros;
        private Panel pnlBotones;
    }
}