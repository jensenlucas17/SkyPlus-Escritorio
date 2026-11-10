namespace SkyPlus.Desktop
{
    partial class frmBackups
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
            pnlSuperior = new Panel();
            lblTitulo = new Label();
            lblSubtitulo = new Label();
            pnlBotones = new FlowLayoutPanel();
            btnGenerar = new Button();
            btnActualizar = new Button();
            btnRestaurar = new Button();
            btnRestaurarProduccion = new Button();
            btnEliminar = new Button();
            dgvBackups = new DataGridView();
            colArchivo = new DataGridViewTextBoxColumn();
            colFecha = new DataGridViewTextBoxColumn();
            colTipo = new DataGridViewTextBoxColumn();
            colEstado = new DataGridViewTextBoxColumn();
            colTamano = new DataGridViewTextBoxColumn();
            colAdministrador = new DataGridViewTextBoxColumn();
            lblEstado = new Label();
            pnlSuperior.SuspendLayout();
            pnlBotones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBackups).BeginInit();
            SuspendLayout();
            // 
            // pnlSuperior
            // 
            pnlSuperior.BackColor = Color.FromArgb(28, 42, 65);
            pnlSuperior.Controls.Add(lblTitulo);
            pnlSuperior.Controls.Add(lblSubtitulo);
            pnlSuperior.Dock = DockStyle.Top;
            pnlSuperior.Location = new Point(0, 0);
            pnlSuperior.Name = "pnlSuperior";
            pnlSuperior.Size = new Size(800, 90);
            pnlSuperior.TabIndex = 0;
            // 
            // lblTitulo
            // 
            lblTitulo.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(12, 9);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(310, 36);
            lblTitulo.TabIndex = 1;
            lblTitulo.Text = "GESTIÓN DE BACKUPS";
            lblTitulo.Click += lblTitulo_Click;
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.ForeColor = Color.White;
            lblSubtitulo.Location = new Point(23, 55);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Size = new Size(216, 15);
            lblSubtitulo.TabIndex = 2;
            lblSubtitulo.Text = "Administración de respaldos de SkyPlus";
            // 
            // pnlBotones
            // 
            pnlBotones.AutoScroll = true;
            pnlBotones.Controls.Add(btnGenerar);
            pnlBotones.Controls.Add(btnActualizar);
            pnlBotones.Controls.Add(btnRestaurar);
            pnlBotones.Controls.Add(btnRestaurarProduccion);
            pnlBotones.Controls.Add(btnEliminar);
            pnlBotones.Dock = DockStyle.Top;
            pnlBotones.Location = new Point(0, 90);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(800, 65);
            pnlBotones.TabIndex = 1;
            pnlBotones.WrapContents = false;
            // 
            // btnGenerar
            // 
            btnGenerar.BackColor = Color.FromArgb(35, 120, 85);
            btnGenerar.FlatAppearance.BorderSize = 0;
            btnGenerar.FlatStyle = FlatStyle.Flat;
            btnGenerar.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnGenerar.ForeColor = Color.White;
            btnGenerar.Location = new Point(3, 3);
            btnGenerar.Name = "btnGenerar";
            btnGenerar.Size = new Size(145, 36);
            btnGenerar.TabIndex = 0;
            btnGenerar.Text = "Generar backup";
            btnGenerar.UseVisualStyleBackColor = false;
            // 
            // btnActualizar
            // 
            btnActualizar.BackColor = Color.FromArgb(45, 105, 170);
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.FlatStyle = FlatStyle.Flat;
            btnActualizar.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnActualizar.ForeColor = Color.White;
            btnActualizar.Location = new Point(154, 3);
            btnActualizar.Name = "btnActualizar";
            btnActualizar.Size = new Size(145, 36);
            btnActualizar.TabIndex = 1;
            btnActualizar.Text = "Actualizar";
            btnActualizar.UseVisualStyleBackColor = false;
            // 
            // btnRestaurar
            // 
            btnRestaurar.BackColor = Color.FromArgb(115, 85, 170);
            btnRestaurar.FlatAppearance.BorderSize = 0;
            btnRestaurar.FlatStyle = FlatStyle.Flat;
            btnRestaurar.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnRestaurar.ForeColor = Color.White;
            btnRestaurar.Location = new Point(305, 3);
            btnRestaurar.Name = "btnRestaurar";
            btnRestaurar.Size = new Size(145, 36);
            btnRestaurar.TabIndex = 2;
            btnRestaurar.Text = "Restaurar prueba";
            btnRestaurar.UseVisualStyleBackColor = false;
            // 
            // btnRestaurarProduccion
            // 
            btnRestaurarProduccion.BackColor = Color.FromArgb(174, 112, 33);
            btnRestaurarProduccion.FlatAppearance.BorderSize = 0;
            btnRestaurarProduccion.FlatStyle = FlatStyle.Flat;
            btnRestaurarProduccion.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnRestaurarProduccion.ForeColor = Color.White;
            btnRestaurarProduccion.Location = new Point(456, 3);
            btnRestaurarProduccion.Name = "btnRestaurarProduccion";
            btnRestaurarProduccion.Size = new Size(165, 36);
            btnRestaurarProduccion.TabIndex = 3;
            btnRestaurarProduccion.Text = "Restaurar producción";
            btnRestaurarProduccion.UseVisualStyleBackColor = false;
            // 
            // btnEliminar
            // 
            btnEliminar.BackColor = Color.FromArgb(185, 65, 65);
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnEliminar.ForeColor = Color.White;
            btnEliminar.Location = new Point(627, 3);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(145, 36);
            btnEliminar.TabIndex = 4;
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseVisualStyleBackColor = false;
            // 
            // dgvBackups
            // 
            dgvBackups.AllowUserToAddRows = false;
            dgvBackups.BackgroundColor = Color.White;
            dgvBackups.ColumnHeadersHeight = 38;
            dgvBackups.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvBackups.Columns.AddRange(new DataGridViewColumn[] { colArchivo, colFecha, colTipo, colEstado, colTamano, colAdministrador });
            dgvBackups.Dock = DockStyle.Fill;
            dgvBackups.Location = new Point(0, 155);
            dgvBackups.Name = "dgvBackups";
            dgvBackups.ReadOnly = true;
            dgvBackups.RowHeadersVisible = false;
            dgvBackups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBackups.Size = new Size(800, 295);
            dgvBackups.TabIndex = 2;
            // 
            // colArchivo
            // 
            colArchivo.DataPropertyName = "Archivo";
            colArchivo.HeaderText = "Archivo";
            colArchivo.Name = "colArchivo";
            colArchivo.ReadOnly = true;
            colArchivo.Width = 133;
            // 
            // colFecha
            // 
            colFecha.DataPropertyName = "FechaLocal";
            colFecha.HeaderText = "Fecha";
            colFecha.Name = "colFecha";
            colFecha.ReadOnly = true;
            colFecha.Width = 133;
            // 
            // colTipo
            // 
            colTipo.DataPropertyName = "Tipo";
            colTipo.HeaderText = "Tipo";
            colTipo.Name = "colTipo";
            colTipo.ReadOnly = true;
            colTipo.Width = 132;
            // 
            // colEstado
            // 
            colEstado.DataPropertyName = "Estado";
            colEstado.HeaderText = "Estado";
            colEstado.Name = "colEstado";
            colEstado.ReadOnly = true;
            colEstado.Width = 133;
            // 
            // colTamano
            // 
            colTamano.DataPropertyName = "TamanoMb";
            colTamano.HeaderText = "Tamaño";
            colTamano.Name = "colTamano";
            colTamano.ReadOnly = true;
            colTamano.Width = 133;
            // 
            // colAdministrador
            // 
            colAdministrador.DataPropertyName = "IdAdministrador";
            colAdministrador.HeaderText = "Administrador";
            colAdministrador.Name = "colAdministrador";
            colAdministrador.ReadOnly = true;
            colAdministrador.Width = 133;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Dock = DockStyle.Bottom;
            lblEstado.Location = new Point(0, 435);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(64, 15);
            lblEstado.TabIndex = 3;
            lblEstado.Text = "Preparado.";
            // 
            // frmBackups
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblEstado);
            Controls.Add(dgvBackups);
            Controls.Add(pnlBotones);
            Controls.Add(pnlSuperior);
            Name = "frmBackups";
            Text = "frmBackups";
            pnlSuperior.ResumeLayout(false);
            pnlSuperior.PerformLayout();
            pnlBotones.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvBackups).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlSuperior;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private FlowLayoutPanel pnlBotones;
        private Button btnGenerar;
        private Button btnActualizar;
        private Label lblEstado;
        private Button btnRestaurar;
        private Button btnRestaurarProduccion;
        private Button btnEliminar;
        private DataGridView dgvBackups;
        private DataGridViewTextBoxColumn colArchivo;
        private DataGridViewTextBoxColumn colFecha;
        private DataGridViewTextBoxColumn colTipo;
        private DataGridViewTextBoxColumn colEstado;
        private DataGridViewTextBoxColumn colTamano;
        private DataGridViewTextBoxColumn colAdministrador;
    }
}