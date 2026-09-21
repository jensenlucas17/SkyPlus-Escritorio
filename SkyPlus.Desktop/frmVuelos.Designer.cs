namespace SkyPlus.Desktop
{
    partial class frmVuelos
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
            panel1 = new Panel();
            btnRefrescar = new Button();
            cmbEstadoFiltro = new ComboBox();
            label1 = new Label();
            txtBuscar = new TextBox();
            panel2 = new Panel();
            btnEliminar = new Button();
            btnEditar = new Button();
            btnNuevo = new Button();
            dgvVuelos = new DataGridView();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVuelos).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(btnRefrescar);
            panel1.Controls.Add(cmbEstadoFiltro);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(txtBuscar);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(800, 100);
            panel1.TabIndex = 0;
            // 
            // btnRefrescar
            // 
            btnRefrescar.Location = new Point(598, 38);
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(75, 23);
            btnRefrescar.TabIndex = 3;
            btnRefrescar.Text = "Buscar";
            btnRefrescar.UseVisualStyleBackColor = true;
            btnRefrescar.Click += btnRefrescar_Click;
            // 
            // cmbEstadoFiltro
            // 
            cmbEstadoFiltro.FormattingEnabled = true;
            cmbEstadoFiltro.Location = new Point(425, 38);
            cmbEstadoFiltro.Name = "cmbEstadoFiltro";
            cmbEstadoFiltro.Size = new Size(121, 23);
            cmbEstadoFiltro.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(194, 20);
            label1.Name = "label1";
            label1.Size = new Size(141, 15);
            label1.TabIndex = 1;
            label1.Text = "Ingrese numero de vuelo:";
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(191, 38);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(186, 23);
            txtBuscar.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Controls.Add(btnEliminar);
            panel2.Controls.Add(btnEditar);
            panel2.Controls.Add(btnNuevo);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 350);
            panel2.Name = "panel2";
            panel2.Size = new Size(800, 100);
            panel2.TabIndex = 1;
            // 
            // btnEliminar
            // 
            btnEliminar.Location = new Point(473, 32);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(132, 31);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text = "Cancelar vuelo";
            btnEliminar.UseVisualStyleBackColor = true;
            // 
            // btnEditar
            // 
            btnEditar.Location = new Point(330, 40);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(75, 23);
            btnEditar.TabIndex = 1;
            btnEditar.Text = "Editar";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnNuevo
            // 
            btnNuevo.Location = new Point(170, 40);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(75, 23);
            btnNuevo.TabIndex = 0;
            btnNuevo.Text = "Nuevo";
            btnNuevo.UseVisualStyleBackColor = true;
            btnNuevo.Click += btnNuevo_Click;
            // 
            // dgvVuelos
            // 
            dgvVuelos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvVuelos.Dock = DockStyle.Fill;
            dgvVuelos.Location = new Point(0, 100);
            dgvVuelos.Name = "dgvVuelos";
            dgvVuelos.Size = new Size(800, 250);
            dgvVuelos.TabIndex = 2;
            // 
            // frmVuelos
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvVuelos);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "frmVuelos";
            Text = "Form1";
            Load += frmVuelos_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvVuelos).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private DataGridView dgvVuelos;
        private TextBox txtBuscar;
        private Label label1;
        private ComboBox cmbEstadoFiltro;
        private Button btnEliminar;
        private Button btnEditar;
        private Button btnNuevo;
        private Button btnRefrescar;
    }
}