namespace SkyPlus.Desktop
{
    partial class frmDashboard
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
            btnCerrarSesion = new Button();
            lblRolActual = new Label();
            lblUsuarioActual = new Label();
            pnlMenu = new FlowLayoutPanel();
            pnlContenido = new Panel();
            pnlSuperior.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSuperior
            // 
            pnlSuperior.Controls.Add(btnCerrarSesion);
            pnlSuperior.Controls.Add(lblRolActual);
            pnlSuperior.Controls.Add(lblUsuarioActual);
            pnlSuperior.Dock = DockStyle.Top;
            pnlSuperior.Location = new Point(0, 0);
            pnlSuperior.Name = "pnlSuperior";
            pnlSuperior.Size = new Size(800, 100);
            pnlSuperior.TabIndex = 0;
            // 
            // btnCerrarSesion
            // 
            btnCerrarSesion.Location = new Point(3, 3);
            btnCerrarSesion.Name = "btnCerrarSesion";
            btnCerrarSesion.Size = new Size(88, 23);
            btnCerrarSesion.TabIndex = 2;
            btnCerrarSesion.Text = "Cerrar sesion";
            btnCerrarSesion.UseVisualStyleBackColor = true;
            btnCerrarSesion.Click += btnCerrarSesion_Click;
            // 
            // lblRolActual
            // 
            lblRolActual.AutoSize = true;
            lblRolActual.Location = new Point(348, 24);
            lblRolActual.Name = "lblRolActual";
            lblRolActual.Size = new Size(59, 15);
            lblRolActual.TabIndex = 1;
            lblRolActual.Text = "Rol actual";
            // 
            // lblUsuarioActual
            // 
            lblUsuarioActual.AutoSize = true;
            lblUsuarioActual.Location = new Point(183, 24);
            lblUsuarioActual.Name = "lblUsuarioActual";
            lblUsuarioActual.Size = new Size(82, 15);
            lblUsuarioActual.TabIndex = 0;
            lblUsuarioActual.Text = "Usuario actual";
            // 
            // pnlMenu
            // 
            pnlMenu.AutoScroll = true;
            pnlMenu.Dock = DockStyle.Left;
            pnlMenu.FlowDirection = FlowDirection.TopDown;
            pnlMenu.Location = new Point(0, 100);
            pnlMenu.Name = "pnlMenu";
            pnlMenu.Size = new Size(200, 350);
            pnlMenu.TabIndex = 1;
            pnlMenu.WrapContents = false;
            // 
            // pnlContenido
            // 
            pnlContenido.Dock = DockStyle.Fill;
            pnlContenido.Location = new Point(200, 100);
            pnlContenido.Name = "pnlContenido";
            pnlContenido.Size = new Size(600, 350);
            pnlContenido.TabIndex = 2;
            // 
            // frmDashboard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pnlContenido);
            Controls.Add(pnlMenu);
            Controls.Add(pnlSuperior);
            Name = "frmDashboard";
            Text = "SkyPlus - Dashboard";
            Load += frmDashboard_Load;
            pnlSuperior.ResumeLayout(false);
            pnlSuperior.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlSuperior;
        private FlowLayoutPanel pnlMenu;
        private Panel pnlContenido;
        private Label lblUsuarioActual;
        private Label lblRolActual;
        private Button btnCerrarSesion;
    }
}