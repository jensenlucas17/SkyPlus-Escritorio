namespace SkyPlus.Desktop
{
    partial class frmUsuarioABM
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
            txtNombre = new TextBox();
            label1 = new Label();
            txtApellido = new TextBox();
            label2 = new Label();
            txtEmail = new TextBox();
            label3 = new Label();
            cmbRol = new ComboBox();
            label4 = new Label();
            txtPassword = new TextBox();
            cmbEstado = new ComboBox();
            lblEstado = new Label();
            lblMensaje = new Label();
            btnGuardar = new Button();
            btnCancelar = new Button();
            lblPassword = new Label();
            lblDNI = new Label();
            txtDni = new TextBox();
            SuspendLayout();
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(202, 21);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(147, 23);
            txtNombre.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(59, 21);
            label1.Name = "label1";
            label1.Size = new Size(54, 15);
            label1.TabIndex = 1;
            label1.Text = "Nombre:";
            // 
            // txtApellido
            // 
            txtApellido.Location = new Point(202, 64);
            txtApellido.Name = "txtApellido";
            txtApellido.Size = new Size(147, 23);
            txtApellido.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(59, 67);
            label2.Name = "label2";
            label2.Size = new Size(54, 15);
            label2.TabIndex = 3;
            label2.Text = "Apellido:";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(202, 158);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(147, 23);
            txtEmail.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(60, 161);
            label3.Name = "label3";
            label3.Size = new Size(39, 15);
            label3.TabIndex = 5;
            label3.Text = "Email:";
            // 
            // cmbRol
            // 
            cmbRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRol.FormattingEnabled = true;
            cmbRol.Location = new Point(202, 204);
            cmbRol.Name = "cmbRol";
            cmbRol.Size = new Size(147, 23);
            cmbRol.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(60, 207);
            label4.Name = "label4";
            label4.Size = new Size(27, 15);
            label4.TabIndex = 7;
            label4.Text = "Rol:";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(202, 304);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(147, 23);
            txtPassword.TabIndex = 11;
            // 
            // cmbEstado
            // 
            cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstado.FormattingEnabled = true;
            cmbEstado.Location = new Point(202, 250);
            cmbEstado.Name = "cmbEstado";
            cmbEstado.Size = new Size(147, 23);
            cmbEstado.TabIndex = 9;
            // 
            // lblEstado
            // 
            lblEstado.AutoSize = true;
            lblEstado.Location = new Point(60, 253);
            lblEstado.Name = "lblEstado";
            lblEstado.Size = new Size(45, 15);
            lblEstado.TabIndex = 11;
            lblEstado.Text = "Estado:";
            // 
            // lblMensaje
            // 
            lblMensaje.AutoSize = true;
            lblMensaje.ForeColor = Color.Red;
            lblMensaje.Location = new Point(59, 369);
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(38, 15);
            lblMensaje.TabIndex = 12;
            lblMensaje.Text = "label7";
            lblMensaje.Visible = false;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(96, 402);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 13;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(224, 402);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 14;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(60, 307);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(70, 15);
            lblPassword.TabIndex = 15;
            lblPassword.Text = "Contraseña:";
            // 
            // lblDNI
            // 
            lblDNI.AutoSize = true;
            lblDNI.Location = new Point(59, 112);
            lblDNI.Name = "lblDNI";
            lblDNI.Size = new Size(27, 15);
            lblDNI.TabIndex = 16;
            lblDNI.Text = "DNI";
            // 
            // txtDni
            // 
            txtDni.Location = new Point(202, 109);
            txtDni.MaxLength = 8;
            txtDni.Name = "txtDni";
            txtDni.Size = new Size(147, 23);
            txtDni.TabIndex = 3;
            // 
            // frmUsuarioABM
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(426, 450);
            Controls.Add(txtDni);
            Controls.Add(lblDNI);
            Controls.Add(lblPassword);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(lblMensaje);
            Controls.Add(lblEstado);
            Controls.Add(cmbEstado);
            Controls.Add(txtPassword);
            Controls.Add(label4);
            Controls.Add(cmbRol);
            Controls.Add(label3);
            Controls.Add(txtEmail);
            Controls.Add(label2);
            Controls.Add(txtApellido);
            Controls.Add(label1);
            Controls.Add(txtNombre);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "frmUsuarioABM";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Form1";
            Load += frmUsuarioABM_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtNombre;
        private Label label1;
        private TextBox txtApellido;
        private Label label2;
        private TextBox txtEmail;
        private Label label3;
        private ComboBox cmbRol;
        private Label label4;
        private TextBox txtPassword;
        private ComboBox cmbEstado;
        private Label lblEstado;
        private Label lblMensaje;
        private Button btnGuardar;
        private Button btnCancelar;
        private Label lblPassword;
        private Label lblDNI;
        private TextBox txtDni;
    }
}