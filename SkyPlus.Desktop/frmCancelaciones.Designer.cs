namespace SkyPlus.Desktop
{
    partial class frmCancelaciones
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
            dgvCancelaciones = new DataGridView();
            lblCancelaciones = new Label();
            pnlFiltros.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCancelaciones).BeginInit();
            SuspendLayout();
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(lblCancelaciones);
            pnlFiltros.Controls.Add(btnBuscar);
            pnlFiltros.Controls.Add(btnRefrescar);
            pnlFiltros.Controls.Add(txtBuscar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 100);
            pnlFiltros.TabIndex = 0;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(454, 43);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(75, 23);
            btnBuscar.TabIndex = 5;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // btnRefrescar
            // 
            btnRefrescar.Location = new Point(557, 43);
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(75, 23);
            btnRefrescar.TabIndex = 6;
            btnRefrescar.Text = "Refrescar";
            btnRefrescar.UseVisualStyleBackColor = true;
            btnRefrescar.Click += btnRefrescar_Click;
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(196, 43);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(167, 23);
            txtBuscar.TabIndex = 0;
            // 
            // dgvCancelaciones
            // 
            dgvCancelaciones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCancelaciones.Dock = DockStyle.Fill;
            dgvCancelaciones.Location = new Point(0, 100);
            dgvCancelaciones.Name = "dgvCancelaciones";
            dgvCancelaciones.ReadOnly = true;
            dgvCancelaciones.Size = new Size(800, 350);
            dgvCancelaciones.TabIndex = 1;
            // 
            // lblCancelaciones
            // 
            lblCancelaciones.AutoSize = true;
            lblCancelaciones.Location = new Point(321, 9);
            lblCancelaciones.Name = "lblCancelaciones";
            lblCancelaciones.Size = new Size(101, 15);
            lblCancelaciones.TabIndex = 2;
            lblCancelaciones.Text = "CANCELACIONES";
            // 
            // frmCancelaciones
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvCancelaciones);
            Controls.Add(pnlFiltros);
            Name = "frmCancelaciones";
            Text = "Form1";
            Load += frmCancelaciones_Load;
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCancelaciones).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlFiltros;
        private TextBox txtBuscar;
        private Button btnBuscar;
        private Button btnRefrescar;
        private DataGridView dgvCancelaciones;
        private Label lblCancelaciones;
    }
}