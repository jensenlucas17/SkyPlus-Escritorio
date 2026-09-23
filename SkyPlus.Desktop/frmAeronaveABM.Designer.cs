namespace SkyPlus.Desktop
{
    partial class frmAeronaveABM
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
            txtMatricula = new TextBox();
            txtModelo = new TextBox();
            label1 = new Label();
            label2 = new Label();
            lblMensaje = new Label();
            btnGuardar = new Button();
            btnCancelar = new Button();
            SuspendLayout();
            // 
            // txtMatricula
            // 
            txtMatricula.Location = new Point(200, 79);
            txtMatricula.Name = "txtMatricula";
            txtMatricula.Size = new Size(137, 23);
            txtMatricula.TabIndex = 0;
            // 
            // txtModelo
            // 
            txtModelo.Location = new Point(198, 129);
            txtModelo.Name = "txtModelo";
            txtModelo.Size = new Size(139, 23);
            txtModelo.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(88, 79);
            label1.Name = "label1";
            label1.Size = new Size(60, 15);
            label1.TabIndex = 2;
            label1.Text = "Matricula:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(88, 129);
            label2.Name = "label2";
            label2.Size = new Size(51, 15);
            label2.TabIndex = 3;
            label2.Text = "Modelo:";
            // 
            // lblMensaje
            // 
            lblMensaje.AutoSize = true;
            lblMensaje.ForeColor = Color.Red;
            lblMensaje.Location = new Point(200, 167);
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(38, 15);
            lblMensaje.TabIndex = 4;
            lblMensaje.Text = "label3";
            lblMensaje.Visible = false;
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(201, 197);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 5;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(307, 197);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 6;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // frmAeronaveABM
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(418, 311);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(lblMensaje);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtModelo);
            Controls.Add(txtMatricula);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "frmAeronaveABM";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Form1";
            Load += frmAeronaveABM_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtMatricula;
        private TextBox txtModelo;
        private Label label1;
        private Label label2;
        private Label lblMensaje;
        private Button btnGuardar;
        private Button btnCancelar;
    }
}