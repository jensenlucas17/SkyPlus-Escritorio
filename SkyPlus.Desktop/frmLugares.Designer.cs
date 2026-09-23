namespace SkyPlus.Desktop
{
    partial class frmLugares
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
            btnRefrescar = new Button();
            txtBuscar = new TextBox();
            pnlBotones = new Panel();
            btnEliminar = new Button();
            btnEditar = new Button();
            btnNuevo = new Button();
            dgvLugares = new DataGridView();
            pnlFiltros.SuspendLayout();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLugares).BeginInit();
            SuspendLayout();
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(label1);
            pnlFiltros.Controls.Add(btnRefrescar);
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
            label1.Location = new Point(200, 18);
            label1.Name = "label1";
            label1.Size = new Size(170, 15);
            label1.TabIndex = 2;
            label1.Text = "Ingrese nombre, pais o codigo:";
            // 
            // btnRefrescar
            // 
            btnRefrescar.Location = new Point(450, 36);
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(75, 23);
            btnRefrescar.TabIndex = 1;
            btnRefrescar.Text = "Buscar";
            btnRefrescar.UseVisualStyleBackColor = true;
            btnRefrescar.Click += btnRefrescar_Click;
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(196, 36);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(182, 23);
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
            btnEliminar.Location = new Point(492, 41);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(75, 23);
            btnEliminar.TabIndex = 2;
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseVisualStyleBackColor = true;
            btnEliminar.Click += btnEliminar_Click;
            // 
            // btnEditar
            // 
            btnEditar.Location = new Point(324, 41);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(75, 23);
            btnEditar.TabIndex = 1;
            btnEditar.Text = "Editar";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnNuevo
            // 
            btnNuevo.Location = new Point(148, 41);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(75, 23);
            btnNuevo.TabIndex = 0;
            btnNuevo.Text = "Nuevo";
            btnNuevo.UseVisualStyleBackColor = true;
            btnNuevo.Click += btnNuevo_Click;
            // 
            // dgvLugares
            // 
            dgvLugares.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLugares.Dock = DockStyle.Fill;
            dgvLugares.Location = new Point(0, 100);
            dgvLugares.MultiSelect = false;
            dgvLugares.Name = "dgvLugares";
            dgvLugares.ReadOnly = true;
            dgvLugares.Size = new Size(800, 250);
            dgvLugares.TabIndex = 2;
            // 
            // frmLugares
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvLugares);
            Controls.Add(pnlBotones);
            Controls.Add(pnlFiltros);
            Name = "frmLugares";
            Text = "Form1";
            Load += frmLugares_Load;
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLugares).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlFiltros;
        private Button btnRefrescar;
        private TextBox txtBuscar;
        private Panel pnlBotones;
        private Button btnEliminar;
        private Button btnEditar;
        private Button btnNuevo;
        private DataGridView dgvLugares;
        private Label label1;
    }
}