namespace SkyPlus.Desktop
{
    partial class frmBoardingPass
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
            dgvBoardingPass = new DataGridView();
            pnlBotones = new Panel();
            btnVerImprimir = new Button();
            pnlFiltros = new Panel();
            btnBuscar = new Button();
            btnRefrescar = new Button();
            txtBuscar = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dgvBoardingPass).BeginInit();
            pnlBotones.SuspendLayout();
            pnlFiltros.SuspendLayout();
            SuspendLayout();
            // 
            // dgvBoardingPass
            // 
            dgvBoardingPass.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvBoardingPass.Dock = DockStyle.Fill;
            dgvBoardingPass.Location = new Point(0, 100);
            dgvBoardingPass.Name = "dgvBoardingPass";
            dgvBoardingPass.ReadOnly = true;
            dgvBoardingPass.Size = new Size(800, 250);
            dgvBoardingPass.TabIndex = 11;
            // 
            // pnlBotones
            // 
            pnlBotones.Controls.Add(btnVerImprimir);
            pnlBotones.Dock = DockStyle.Bottom;
            pnlBotones.Location = new Point(0, 350);
            pnlBotones.Name = "pnlBotones";
            pnlBotones.Size = new Size(800, 100);
            pnlBotones.TabIndex = 10;
            // 
            // btnVerImprimir
            // 
            btnVerImprimir.Location = new Point(337, 29);
            btnVerImprimir.Name = "btnVerImprimir";
            btnVerImprimir.Size = new Size(103, 27);
            btnVerImprimir.TabIndex = 0;
            btnVerImprimir.Text = "Imprimir";
            btnVerImprimir.UseVisualStyleBackColor = true;
            btnVerImprimir.Click += btnVerImprimir_Click;
            // 
            // pnlFiltros
            // 
            pnlFiltros.Controls.Add(btnBuscar);
            pnlFiltros.Controls.Add(btnRefrescar);
            pnlFiltros.Controls.Add(txtBuscar);
            pnlFiltros.Dock = DockStyle.Top;
            pnlFiltros.Location = new Point(0, 0);
            pnlFiltros.Name = "pnlFiltros";
            pnlFiltros.Size = new Size(800, 100);
            pnlFiltros.TabIndex = 9;
            // 
            // btnBuscar
            // 
            btnBuscar.Location = new Point(440, 39);
            btnBuscar.Name = "btnBuscar";
            btnBuscar.Size = new Size(75, 23);
            btnBuscar.TabIndex = 8;
            btnBuscar.Text = "Buscar";
            btnBuscar.UseVisualStyleBackColor = true;
            btnBuscar.Click += btnBuscar_Click;
            // 
            // btnRefrescar
            // 
            btnRefrescar.Location = new Point(543, 39);
            btnRefrescar.Name = "btnRefrescar";
            btnRefrescar.Size = new Size(75, 23);
            btnRefrescar.TabIndex = 9;
            btnRefrescar.Text = "Refrescar";
            btnRefrescar.UseVisualStyleBackColor = true;
            btnRefrescar.Click += btnRefrescar_Click;
            // 
            // txtBuscar
            // 
            txtBuscar.Location = new Point(182, 39);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.Size = new Size(167, 23);
            txtBuscar.TabIndex = 7;
            // 
            // frmBoardingPass
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dgvBoardingPass);
            Controls.Add(pnlBotones);
            Controls.Add(pnlFiltros);
            Name = "frmBoardingPass";
            Text = "Form1";
            Load += frmBoardingPass_Load;
            ((System.ComponentModel.ISupportInitialize)dgvBoardingPass).EndInit();
            pnlBotones.ResumeLayout(false);
            pnlFiltros.ResumeLayout(false);
            pnlFiltros.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgvBoardingPass;
        private Panel pnlBotones;
        private Button btnVerImprimir;
        private Panel pnlFiltros;
        private Button btnBuscar;
        private Button btnRefrescar;
        private TextBox txtBuscar;
    }
}