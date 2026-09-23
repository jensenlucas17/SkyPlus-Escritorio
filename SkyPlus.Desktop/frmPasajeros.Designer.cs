namespace SkyPlus.Desktop
{
    partial class frmPasajeros
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
            label1 = new Label();
            txtBuscar = new TextBox();
            pnlBotones = new Panel();
            btnEliminar = new Button();
            btnEditar = new Button();
            btnNuevo = new Button();
            dgvPasajeros = new DataGridView();
            pnlFiltros.SuspendLayout();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPasajeros).BeginInit();
            SuspendLayout();
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(btnBuscar);
            pnlFiltros.Controls.Add(label1);
            pnlFiltros.Controls.Add(txtBuscar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 100);
            pnlFiltros.TabIndex = 0;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(439, 35);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(75, 23);
            btnBuscar.TabIndex = 2;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(218, 18);
            label1.Name = "label1";
            label1.Size = new Size(126, 15);
            label1.TabIndex = 1;
            label1.Text = "Ingrese apellido o DNI:";
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(214, 36);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(163, 23);
            txtBuscar.TabIndex = 0;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(btnEliminar);
            pnlBotones.Controls.Add(btnEditar);
            pnlBotones.Controls.Add(btnNuevo);
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Location = new Point(0, 350);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(800, 100);
            pnlBotones.TabIndex = 1;
            // 
            // btnEliminar
            // 
            btnEliminar.Location = new Point(503, 30);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(75, 23);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // btnEditar
            // 
            btnEditar.Location = new Point(357, 30);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(75, 23);
            btnEditar.TabIndex = 1;
            btnEditar.Text = "Editar";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnNuevo
            // 
            btnNuevo.Location = new Point(209, 30);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(75, 23);
            btnNuevo.TabIndex = 0;
            btnNuevo.Text = "Nuevo";
            btnNuevo.UseVisualStyleBackColor = true;
            btnNuevo.Click += btnNuevo_Click;
            // 
            // dgvPasajeros
            // 
            dgvPasajeros.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPasajeros.Dock = DockStyle.Fill;
            dgvPasajeros.Location = new Point(0, 100);
            dgvPasajeros.Name = "dgvPasajeros";
            dgvPasajeros.ReadOnly = true;
            dgvPasajeros.Size = new Size(800, 250);
            dgvPasajeros.TabIndex = 2;
            // 
            // frmPasajeros
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvPasajeros);
            Controls.Add(pnlBotones);
            Controls.Add(pnlFiltros);
            Name = "frmPasajeros";
            Text = "Form1";
            Load += frmPasajeros_Load;
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPasajeros).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlFiltros;
        private TextBox txtBuscar;
        private Label label1;
        private Button btnBuscar;
        private Panel pnlBotones;
        private Button btnEliminar;
        private Button btnEditar;
        private Button btnNuevo;
        private DataGridView dgvPasajeros;
    }
}