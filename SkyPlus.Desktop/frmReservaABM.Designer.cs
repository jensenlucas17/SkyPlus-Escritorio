namespace SkyPlus.Desktop
{
    partial class frmReservaABM
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
            cmbPasajero = new ComboBox();
            cmbVuelo = new ComboBox();
            cmbAsiento = new ComboBox();
            lblTarifa = new Label();
            lblMensaje = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnGuardar = new Button();
            btnCancelar = new Button();
            SuspendLayout();
            // 
            // cmbPasajero
            // 
            cmbPasajero.FormattingEnabled = true;
            cmbPasajero.Location = new Point(234, 78);
            cmbPasajero.Name = "cmbPasajero";
            cmbPasajero.Size = new Size(159, 23);
            cmbPasajero.TabIndex = 0;
            // 
            // cmbVuelo
            // 
            cmbVuelo.FormattingEnabled = true;
            cmbVuelo.Location = new Point(234, 135);
            cmbVuelo.Name = "cmbVuelo";
            cmbVuelo.Size = new Size(159, 23);
            cmbVuelo.TabIndex = 1;
            // 
            // cmbAsiento
            // 
            cmbAsiento.FormattingEnabled = true;
            cmbAsiento.Location = new Point(234, 194);
            cmbAsiento.Name = "cmbAsiento";
            cmbAsiento.Size = new Size(159, 23);
            cmbAsiento.TabIndex = 2;
            // 
            // lblTarifa
            // 
            lblTarifa.AutoSize = true;
            lblTarifa.Location = new Point(234, 237);
            lblTarifa.Name = "lblTarifa";
            lblTarifa.Size = new Size(38, 15);
            lblTarifa.TabIndex = 3;
            lblTarifa.Text = "label1";
            // 
            // lblMensaje
            // 
            lblMensaje.AutoSize = true;
            lblMensaje.ForeColor = Color.Red;
            lblMensaje.Location = new Point(234, 277);
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(38, 15);
            lblMensaje.TabIndex = 4;
            lblMensaje.Text = "label1";
            lblMensaje.Visible = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(120, 78);
            label1.Name = "label1";
            label1.Size = new Size(54, 15);
            label1.TabIndex = 5;
            label1.Text = "Pasajero:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(120, 135);
            label2.Name = "label2";
            label2.Size = new Size(40, 15);
            label2.TabIndex = 6;
            label2.Text = "Vuelo:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(120, 194);
            label3.Name = "label3";
            label3.Size = new Size(50, 15);
            label3.TabIndex = 7;
            label3.Text = "Asiento:";
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(163, 317);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 8;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(318, 317);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 9;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // frmReservaABM
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(572, 450);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lblMensaje);
            Controls.Add(lblTarifa);
            Controls.Add(cmbAsiento);
            Controls.Add(cmbVuelo);
            Controls.Add(cmbPasajero);
            Name = "frmReservaABM";
            Text = "Form1";
            Load += frmReservaABM_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cmbPasajero;
        private ComboBox cmbVuelo;
        private ComboBox cmbAsiento;
        private Label lblTarifa;
        private Label lblMensaje;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button btnGuardar;
        private Button btnCancelar;
        private Button button3;
    }
}