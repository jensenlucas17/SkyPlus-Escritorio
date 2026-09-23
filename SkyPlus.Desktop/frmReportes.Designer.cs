namespace SkyPlus.Desktop
{
    partial class frmReportes
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
            dgvReporte = new DataGridView();
            pnlBotones = new Panel();
            lblResultado = new Label();
            pnlFiltros = new Panel();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            dtpHasta = new DateTimePicker();
            dtpDesde = new DateTimePicker();
            cmbTipoReporte = new ComboBox();
            btnGenerar = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvReporte).BeginInit();
            pnlBotones.SuspendLayout();
            pnlFiltros.SuspendLayout();
            SuspendLayout();
            // 
            // dgvReporte
            // 
            dgvReporte.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvReporte.Dock = DockStyle.Fill;
            dgvReporte.Location = new Point(0, 126);
            dgvReporte.Name = "dgvReporte";
            dgvReporte.ReadOnly = true;
            dgvReporte.Size = new Size(800, 224);
            dgvReporte.TabIndex = 14;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(lblResultado);
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Location = new Point(0, 350);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(800, 100);
            pnlBotones.TabIndex = 13;
            // 
            // lblResultado
            // 
            lblResultado.AutoSize = true;
            lblResultado.Location = new Point(350, 23);
            lblResultado.Name = "lblResultado";
            lblResultado.Size = new Size(38, 15);
            lblResultado.TabIndex = 0;
            lblResultado.Text = "label4";
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(label3);
            pnlFiltros.Controls.Add(label2);
            pnlFiltros.Controls.Add(label1);
            pnlFiltros.Controls.Add(dtpHasta);
            pnlFiltros.Controls.Add(dtpDesde);
            pnlFiltros.Controls.Add(cmbTipoReporte);
            pnlFiltros.Controls.Add(btnGenerar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 126);
            pnlFiltros.TabIndex = 12;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(99, 12);
            label3.Name = "label3";
            label3.Size = new Size(75, 15);
            label3.TabIndex = 15;
            label3.Text = "Tipo reporte:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(132, 54);
            label2.Name = "label2";
            label2.Size = new Size(42, 15);
            label2.TabIndex = 14;
            label2.Text = "Desde:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(132, 97);
            label1.Name = "label1";
            label1.Size = new Size(40, 15);
            label1.TabIndex = 13;
            label1.Text = "Hasta:";
            // 
            // dtpHasta
            // 
            dtpHasta.Format = DateTimePickerFormat.Short;
            dtpHasta.Location = new Point(195, 97);
            dtpHasta.Name = "dtpHasta";
            dtpHasta.Size = new Size(167, 23);
            dtpHasta.TabIndex = 12;
            // 
            // dtpDesde
            // 
            dtpDesde.Format = DateTimePickerFormat.Short;
            dtpDesde.Location = new Point(195, 54);
            dtpDesde.Name = "dtpDesde";
            dtpDesde.Size = new Size(167, 23);
            dtpDesde.TabIndex = 11;
            // 
            // cmbTipoReporte
            // 
            cmbTipoReporte.FormattingEnabled = true;
            cmbTipoReporte.Location = new Point(195, 12);
            cmbTipoReporte.Name = "cmbTipoReporte";
            cmbTipoReporte.Size = new Size(167, 23);
            cmbTipoReporte.TabIndex = 10;
            // 
            // btnGenerar
            // 
            btnGenerar.Location = new Point(447, 46);
            btnGenerar.Name = "btnGenerar";
            btnGenerar.Size = new Size(87, 31);
            btnGenerar.TabIndex = 8;
            btnGenerar.Text = "Generar";
            btnGenerar.UseVisualStyleBackColor = true;
            btnGenerar.Click += btnGenerar_Click;
            // 
            // frmReportes
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvReporte);
            Controls.Add(pnlBotones);
            Controls.Add(pnlFiltros);
            Name = "frmReportes";
            Text = "Form1";
            Load += frmReportes_Load;
            ((System.ComponentModel.ISupportInitialize)dgvReporte).EndInit();
            pnlBotones.ResumeLayout(false);
            pnlBotones.PerformLayout();
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvReporte;
        private Panel pnlBotones;
        private Panel pnlFiltros;
        private ComboBox cmbTipoReporte;
        private Button btnGenerar;
        private DateTimePicker dtpDesde;
        private DateTimePicker dtpHasta;
        private Label label2;
        private Label label1;
        private Label label3;
        private Label lblResultado;
    }
}