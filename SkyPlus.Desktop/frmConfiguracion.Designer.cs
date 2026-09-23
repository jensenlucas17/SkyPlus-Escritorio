namespace SkyPlus.Desktop
{
    partial class frmConfiguracion
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
            grpDatosGenerales = new GroupBox();
            btnGuardarGeneral = new Button();
            cmbMoneda = new ComboBox();
            txtNombreAerolinea = new TextBox();
            label2 = new Label();
            label1 = new Label();
            grpSeguridad = new GroupBox();
            btnCambiarPassword = new Button();
            txtPasswordNueva = new TextBox();
            txtPasswordActual = new TextBox();
            label5 = new Label();
            label4 = new Label();
            lblUsuarioActual = new Label();
            grpDatosGenerales.SuspendLayout();
            grpSeguridad.SuspendLayout();
            SuspendLayout();
            // 
            // grpDatosGenerales
            // 
            grpDatosGenerales.Controls.Add(btnGuardarGeneral);
            grpDatosGenerales.Controls.Add(cmbMoneda);
            grpDatosGenerales.Controls.Add(txtNombreAerolinea);
            grpDatosGenerales.Controls.Add(label2);
            grpDatosGenerales.Controls.Add(label1);
            grpDatosGenerales.Location = new Point(246, 43);
            grpDatosGenerales.Name = "grpDatosGenerales";
            grpDatosGenerales.Size = new Size(244, 174);
            grpDatosGenerales.TabIndex = 0;
            grpDatosGenerales.TabStop = false;
            grpDatosGenerales.Text = "Datos generales de la aerolinea";
            // 
            // btnGuardarGeneral
            // 
            btnGuardarGeneral.Location = new Point(57, 131);
            btnGuardarGeneral.Name = "btnGuardarGeneral";
            btnGuardarGeneral.Size = new Size(133, 23);
            btnGuardarGeneral.TabIndex = 4;
            btnGuardarGeneral.Text = "Guardar cambios";
            btnGuardarGeneral.UseVisualStyleBackColor = true;
            btnGuardarGeneral.Click += btnGuardarGeneral_Click;
            // 
            // cmbMoneda
            // 
            cmbMoneda.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMoneda.FormattingEnabled = true;
            cmbMoneda.Location = new Point(82, 91);
            cmbMoneda.Name = "cmbMoneda";
            cmbMoneda.Size = new Size(133, 23);
            cmbMoneda.TabIndex = 3;
            // 
            // txtNombreAerolinea
            // 
            txtNombreAerolinea.Location = new Point(82, 30);
            txtNombreAerolinea.Name = "txtNombreAerolinea";
            txtNombreAerolinea.Size = new Size(133, 23);
            txtNombreAerolinea.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 84);
            label2.Name = "label2";
            label2.Size = new Size(57, 15);
            label2.TabIndex = 1;
            label2.Text = "Moneda: ";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 30);
            label1.Name = "label1";
            label1.Size = new Size(54, 15);
            label1.TabIndex = 0;
            label1.Text = "Nombre:";
            // 
            // grpSeguridad
            // 
            grpSeguridad.Controls.Add(btnCambiarPassword);
            grpSeguridad.Controls.Add(txtPasswordNueva);
            grpSeguridad.Controls.Add(txtPasswordActual);
            grpSeguridad.Controls.Add(label5);
            grpSeguridad.Controls.Add(label4);
            grpSeguridad.Controls.Add(lblUsuarioActual);
            grpSeguridad.Location = new Point(246, 241);
            grpSeguridad.Name = "grpSeguridad";
            grpSeguridad.Size = new Size(263, 185);
            grpSeguridad.TabIndex = 1;
            grpSeguridad.TabStop = false;
            grpSeguridad.Text = "Seguridad";
            // 
            // btnCambiarPassword
            // 
            btnCambiarPassword.Location = new Point(57, 150);
            btnCambiarPassword.Name = "btnCambiarPassword";
            btnCambiarPassword.Size = new Size(133, 23);
            btnCambiarPassword.TabIndex = 5;
            btnCambiarPassword.Text = "Cambiar contraseña";
            btnCambiarPassword.UseVisualStyleBackColor = true;
            btnCambiarPassword.Click += btnCambiarPassword_Click;
            // 
            // txtPasswordNueva
            // 
            txtPasswordNueva.Location = new Point(122, 121);
            txtPasswordNueva.Name = "txtPasswordNueva";
            txtPasswordNueva.PasswordChar = '●';
            txtPasswordNueva.Size = new Size(135, 23);
            txtPasswordNueva.TabIndex = 4;
            // 
            // txtPasswordActual
            // 
            txtPasswordActual.Location = new Point(122, 73);
            txtPasswordActual.Name = "txtPasswordActual";
            txtPasswordActual.PasswordChar = '●';
            txtPasswordActual.Size = new Size(135, 23);
            txtPasswordActual.TabIndex = 3;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(6, 121);
            label5.Name = "label5";
            label5.Size = new Size(105, 15);
            label5.TabIndex = 2;
            label5.Text = "Contraseña nueva:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(6, 81);
            label4.Name = "label4";
            label4.Size = new Size(105, 15);
            label4.TabIndex = 1;
            label4.Text = "Contraseña actual:";
            // 
            // lblUsuarioActual
            // 
            lblUsuarioActual.AutoSize = true;
            lblUsuarioActual.Location = new Point(6, 40);
            lblUsuarioActual.Name = "lblUsuarioActual";
            lblUsuarioActual.Size = new Size(38, 15);
            lblUsuarioActual.TabIndex = 0;
            lblUsuarioActual.Text = "label3";
            // 
            // frmConfiguracion
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(grpSeguridad);
            Controls.Add(grpDatosGenerales);
            Name = "frmConfiguracion";
            Text = "Form1";
            Load += frmConfiguracion_Load;
            grpDatosGenerales.ResumeLayout(false);
            grpDatosGenerales.PerformLayout();
            grpSeguridad.ResumeLayout(false);
            grpSeguridad.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpDatosGenerales;
        private GroupBox grpSeguridad;
        private Label label2;
        private Label label1;
        private TextBox txtNombreAerolinea;
        private ComboBox cmbMoneda;
        private Button btnGuardarGeneral;
        private Label label5;
        private Label label4;
        private Label lblUsuarioActual;
        private TextBox txtPasswordActual;
        private TextBox txtPasswordNueva;
        private Button btnCambiarPassword;
    }
}