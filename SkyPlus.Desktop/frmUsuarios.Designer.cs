namespace SkyPlus.Desktop
{
    partial class frmUsuarios
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
            lblEstadoFiltro = new Label();
            lblRolFiltro = new Label();
            btnRefrescar = new Button();
            cmbEstadoFiltro = new ComboBox();
            cmbRolFiltro = new ComboBox();
            txtBuscar = new TextBox();
            panel1 = new Panel();
            btnDesactivarReactivar = new Button();
            btnEditar = new Button();
            btnNuevo = new Button();
            dgvUsuarios = new DataGridView();
            pnlFiltros.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).BeginInit();
            SuspendLayout();
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(lblEstadoFiltro);
            pnlFiltros.Controls.Add(lblRolFiltro);
            pnlFiltros.Controls.Add(btnRefrescar);
            pnlFiltros.Controls.Add(cmbEstadoFiltro);
            pnlFiltros.Controls.Add(cmbRolFiltro);
            pnlFiltros.Controls.Add(txtBuscar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 100);
            pnlFiltros.TabIndex = 0;
            // 
            // lblEstadoFiltro
            // 
            lblEstadoFiltro.AutoSize = true;
            lblEstadoFiltro.Location = new Point(305, 52);
            lblEstadoFiltro.Name = "lblEstadoFiltro";
            lblEstadoFiltro.Size = new Size(45, 15);
            lblEstadoFiltro.TabIndex = 4;
            lblEstadoFiltro.Text = "Estado:";
            // 
            // lblRolFiltro
            // 
            lblRolFiltro.AutoSize = true;
            lblRolFiltro.Location = new Point(120, 52);
            lblRolFiltro.Name = "lblRolFiltro";
            lblRolFiltro.Size = new Size(27, 15);
            lblRolFiltro.TabIndex = 3;
            lblRolFiltro.Text = "Rol:";
            // 
            // btnRefrescar
            // 
            btnRefrescar.Location = new Point(528, 48);
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(75, 23);
            btnRefrescar.TabIndex = 3;
            btnRefrescar.Text = "Refrescar";
            btnRefrescar.UseVisualStyleBackColor = true;
            btnRefrescar.Click += btnRefrescar_Click;
            // 
            // cmbEstadoFiltro
            // 
            cmbEstadoFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstadoFiltro.FormattingEnabled = true;
            cmbEstadoFiltro.Location = new Point(356, 49);
            cmbEstadoFiltro.Name = "cmbEstadoFiltro";
            cmbEstadoFiltro.Size = new Size(121, 23);
            cmbEstadoFiltro.TabIndex = 2;
            cmbEstadoFiltro.SelectedIndexChanged += cmbEstadoFiltro_SelectedIndexChanged;
            // 
            // cmbRolFiltro
            // 
            cmbRolFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRolFiltro.FormattingEnabled = true;
            cmbRolFiltro.Location = new Point(153, 48);
            cmbRolFiltro.Name = "cmbRolFiltro";
            cmbRolFiltro.Size = new Size(121, 23);
            cmbRolFiltro.TabIndex = 1;
            cmbRolFiltro.SelectedIndexChanged += cmbRolFiltro_SelectedIndexChanged;
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(235, 12);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Buscar por nombre o email";
            txtBuscar.Size = new Size(151, 23);
            txtBuscar.TabIndex = 0;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnDesactivarReactivar);
            panel1.Controls.Add(btnEditar);
            panel1.Controls.Add(btnNuevo);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 350);
            panel1.Name = "panel1";
            panel1.Size = new Size(800, 100);
            panel1.TabIndex = 1;
            // 
            // btnDesactivarReactivar
            // 
            btnDesactivarReactivar.Location = new Point(417, 37);
            btnDesactivarReactivar.Name = "btnDesactivarReactivar";
            btnDesactivarReactivar.Size = new Size(121, 23);
            btnDesactivarReactivar.TabIndex = 2;
            btnDesactivarReactivar.Text = "Desactivar/Activar";
            btnDesactivarReactivar.UseVisualStyleBackColor = true;
            btnDesactivarReactivar.Click += btnDesactivarReactivar_Click;
            // 
            // btnEditar
            // 
            btnEditar.Location = new Point(288, 37);
            btnEditar.Name = "btnEditar";
            btnEditar.Size = new Size(75, 23);
            btnEditar.TabIndex = 1;
            btnEditar.Text = "Editar";
            btnEditar.UseVisualStyleBackColor = true;
            btnEditar.Click += btnEditar_Click;
            // 
            // btnNuevo
            // 
            btnNuevo.Location = new Point(153, 37);
            btnNuevo.Name = "btnNuevo";
            btnNuevo.Size = new Size(75, 23);
            btnNuevo.TabIndex = 0;
            btnNuevo.Text = "Nuevo";
            btnNuevo.UseVisualStyleBackColor = true;
            btnNuevo.Click += btnNuevo_Click;
            // 
            // dgvUsuarios
            // 
            dgvUsuarios.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUsuarios.Dock = DockStyle.Fill;
            dgvUsuarios.Location = new Point(0, 100);
            dgvUsuarios.MultiSelect = false;
            dgvUsuarios.Name = "dgvUsuarios";
            dgvUsuarios.ReadOnly = true;
            dgvUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsuarios.Size = new Size(800, 250);
            dgvUsuarios.TabIndex = 2;
            // 
            // frmUsuarios
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvUsuarios);
            Controls.Add(panel1);
            Controls.Add(pnlFiltros);
            Name = "frmUsuarios";
            Text = "Form1";
            Load += frmUsuarios_Load;
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvUsuarios).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlFiltros;
        private ComboBox cmbEstadoFiltro;
        private ComboBox cmbRolFiltro;
        private TextBox txtBuscar;
        private Button btnRefrescar;
        private Panel panel1;
        private Button btnDesactivarReactivar;
        private Button btnEditar;
        private Button btnNuevo;
        private DataGridView dgvUsuarios;
        private Label lblRolFiltro;
        private Label lblEstadoFiltro;
    }
}