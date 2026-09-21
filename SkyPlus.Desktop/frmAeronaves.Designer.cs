namespace SkyPlus.Desktop
{
    partial class frmAeronaves
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
            label1 = new Label();
            btnBuscar = new Button();
            txtBuscar = new TextBox();
            pnlBotones = new Panel();
            btnEliminar = new Button();
            btnEditar = new Button();
            btnNuevo = new Button();
            dgvAeronaves = new DataGridView();
            pnlFiltros.SuspendLayout();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAeronaves).BeginInit();
            SuspendLayout();
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(label1);
            pnlFiltros.Controls.Add(btnBuscar);
            pnlFiltros.Controls.Add(txtBuscar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 100);
            pnlFiltros.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(199, 16);
            label1.Name = "label1";
            label1.Size = new Size(101, 15);
            label1.TabIndex = 2;
            label1.Text = "Ingrese matricula:";
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(459, 34);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(75, 23);
            btnBuscar.TabIndex = 1;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnActualizar_Click;
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(195, 34);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(187, 23);
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
            btnEliminar.Location = new Point(497, 40);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(75, 23);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // btnEditar
            // 
            btnEditar.Location = new Point(343, 40);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(75, 23);
            btnEditar.TabIndex = 1;
            btnEditar.Text = "Editar";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnNuevo
            // 
            btnNuevo.Location = new Point(182, 40);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(75, 23);
            btnNuevo.TabIndex = 0;
            btnNuevo.Text = "Nuevo";
            btnNuevo.UseVisualStyleBackColor = true;
            btnNuevo.Click += btnNuevo_Click;
            // 
            // dgvAeronaves
            // 
            dgvAeronaves.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAeronaves.Dock = DockStyle.Fill;
            dgvAeronaves.Location = new Point(0, 100);
            dgvAeronaves.Name = "dgvAeronaves";
            dgvAeronaves.ReadOnly = true;
            dgvAeronaves.Size = new Size(800, 250);
            dgvAeronaves.TabIndex = 2;
            // 
            // frmAeronaves
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvAeronaves);
            Controls.Add(pnlBotones);
            Controls.Add(pnlFiltros);
            Name = "frmAeronaves";
            Text = "Form1";
            Load += frmAeronaves_Load;
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvAeronaves).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlFiltros;
        private Panel pnlBotones;
        private DataGridView dgvAeronaves;
        private TextBox txtBuscar;
        private Button btnBuscar;
        private Button btnEliminar;
        private Button btnEditar;
        private Button btnNuevo;
        private Label label1;
    }
}