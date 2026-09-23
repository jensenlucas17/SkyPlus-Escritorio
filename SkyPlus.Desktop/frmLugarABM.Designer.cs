namespace SkyPlus.Desktop
{
    partial class frmLugarABM
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
            txtCodigoIata = new TextBox();
            txtNombre = new TextBox();
            txtCiudad = new TextBox();
            txtPais = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            lblMensaje = new Label();
            btnGuardar = new Button();
            btnCancelar = new Button();
            SuspendLayout();
            // 
            // txtCodigoIata
            // 
            txtCodigoIata.Location = new Point(161, 52);
            txtCodigoIata.Name = "txtCodigoIata";
            txtCodigoIata.Size = new Size(144, 23);
            txtCodigoIata.TabIndex = 0;
            // 
            // txtNombre
            // 
            txtNombre.Location = new Point(161, 106);
            txtNombre.Name = "txtNombre";
            txtNombre.Size = new Size(144, 23);
            txtNombre.TabIndex = 1;
            // 
            // txtCiudad
            // 
            txtCiudad.Location = new Point(161, 160);
            txtCiudad.Name = "txtCiudad";
            txtCiudad.Size = new Size(144, 23);
            txtCiudad.TabIndex = 2;
            // 
            // txtPais
            // 
            txtPais.Location = new Point(161, 216);
            txtPais.Name = "txtPais";
            txtPais.Size = new Size(144, 23);
            txtPais.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(44, 52);
            label1.Name = "label1";
            label1.Size = new Size(49, 15);
            label1.TabIndex = 5;
            label1.Text = "Codigo:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(44, 102);
            label2.Name = "label2";
            label2.Size = new Size(54, 15);
            label2.TabIndex = 6;
            label2.Text = "Nombre:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(44, 160);
            label3.Name = "label3";
            label3.Size = new Size(48, 15);
            label3.TabIndex = 7;
            label3.Text = "Ciudad:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(49, 216);
            label4.Name = "label4";
            label4.Size = new Size(31, 15);
            label4.TabIndex = 8;
            label4.Text = "Pais:";
            // 
            // lblMensaje
            // 
            lblMensaje.AutoSize = true;
            lblMensaje.ForeColor = Color.Red;
            lblMensaje.Location = new Point(161, 262);
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(38, 15);
            lblMensaje.TabIndex = 9;
            lblMensaje.Text = "label5";
            lblMensaje.Visible = false;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(170, 323);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 10;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(311, 323);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 11;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // frmLugarABM
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(443, 450);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(lblMensaje);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtPais);
            Controls.Add(txtCiudad);
            Controls.Add(txtNombre);
            Controls.Add(txtCodigoIata);
            Name = "frmLugarABM";
            Text = "Form1";
            Load += frmLugarABM_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtCodigoIata;
        private TextBox txtNombre;
        private TextBox txtCiudad;
        private TextBox txtPais;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label lblMensaje;
        private Button btnGuardar;
        private Button btnCancelar;
    }
}