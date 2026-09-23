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
            lblTarifa = new Label();
            lblMensaje = new Label();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            btnGuardar = new Button();
            btnCancelar = new Button();
            label4 = new Label();
            label5 = new Label();
            btnNuevoPasajero = new Button();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            cmbOrigen = new ComboBox();
            cmbDestino = new ComboBox();
            flpAsientos = new FlowLayoutPanel();
            label9 = new Label();
            SuspendLayout();
            // 
            // cmbPasajero
            // 
            cmbPasajero.FormattingEnabled = true;
            cmbPasajero.Location = new Point(113, 60);
            cmbPasajero.Name = "cmbPasajero";
            cmbPasajero.Size = new Size(159, 23);
            cmbPasajero.TabIndex = 0;
            // 
            // cmbVuelo
            // 
            cmbVuelo.FormattingEnabled = true;
            cmbVuelo.Location = new Point(113, 192);
            cmbVuelo.Name = "cmbVuelo";
            cmbVuelo.Size = new Size(159, 23);
            cmbVuelo.TabIndex = 1;
            cmbVuelo.SelectedIndexChanged += cmbVuelo_SelectedIndexChanged;
            // 
            // lblTarifa
            // 
            lblTarifa.AutoSize = true;
            lblTarifa.Location = new Point(398, 383);
            lblTarifa.Name = "lblTarifa";
            lblTarifa.Size = new Size(57, 15);
            lblTarifa.TabIndex = 3;
            lblTarifa.Text = "TOTAL: $0";
            // 
            // lblMensaje
            // 
            lblMensaje.AutoSize = true;
            lblMensaje.ForeColor = Color.Red;
            lblMensaje.Location = new Point(222, 383);
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(38, 15);
            lblMensaje.TabIndex = 4;
            lblMensaje.Text = "label1";
            lblMensaje.Visible = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(33, 38);
            label1.Name = "label1";
            label1.Size = new Size(62, 15);
            label1.TabIndex = 5;
            label1.Text = "PASAJERO";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(33, 109);
            label2.Name = "label2";
            label2.Size = new Size(43, 15);
            label2.TabIndex = 6;
            label2.Text = "VUELO";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(33, 237);
            label3.Name = "label3";
            label3.Size = new Size(53, 15);
            label3.TabIndex = 7;
            label3.Text = "ASIENTO";
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(378, 406);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 8;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            btnGuardar.Click += btnGuardar_Click;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(297, 406);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 9;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            btnCancelar.Click += btnCancelar_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(33, 9);
            label4.Name = "label4";
            label4.Size = new Size(93, 15);
            label4.TabIndex = 10;
            label4.Text = "NUEVA RESERVA";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(41, 63);
            label5.Name = "label5";
            label5.Size = new Size(54, 15);
            label5.TabIndex = 11;
            label5.Text = "Pasajero:";
            // 
            // btnNuevoPasajero
            // 
            btnNuevoPasajero.Location = new Point(297, 59);
            btnNuevoPasajero.Name = "btnNuevoPasajero";
            btnNuevoPasajero.Size = new Size(139, 23);
            btnNuevoPasajero.TabIndex = 12;
            btnNuevoPasajero.Text = "+ Nuevo Pasajero";
            btnNuevoPasajero.UseVisualStyleBackColor = true;
            btnNuevoPasajero.Click += btnNuevoPasajero_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(41, 135);
            label6.Name = "label6";
            label6.Size = new Size(46, 15);
            label6.TabIndex = 13;
            label6.Text = "Origen:";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(41, 166);
            label7.Name = "label7";
            label7.Size = new Size(50, 15);
            label7.TabIndex = 14;
            label7.Text = "Destino:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(41, 195);
            label8.Name = "label8";
            label8.Size = new Size(40, 15);
            label8.TabIndex = 15;
            label8.Text = "Vuelo:";
            // 
            // cmbOrigen
            // 
            cmbOrigen.FormattingEnabled = true;
            cmbOrigen.Location = new Point(113, 132);
            cmbOrigen.Name = "cmbOrigen";
            cmbOrigen.Size = new Size(159, 23);
            cmbOrigen.TabIndex = 16;
            cmbOrigen.SelectedIndexChanged += cmbOrigen_SelectedIndexChanged;
            // 
            // cmbDestino
            // 
            cmbDestino.FormattingEnabled = true;
            cmbDestino.Location = new Point(113, 163);
            cmbDestino.Name = "cmbDestino";
            cmbDestino.Size = new Size(159, 23);
            cmbDestino.TabIndex = 17;
            cmbDestino.SelectedIndexChanged += cmbDestino_SelectedIndexChanged;
            // 
            // flpAsientos
            // 
            flpAsientos.AutoScroll = true;
            flpAsientos.Location = new Point(33, 255);
            flpAsientos.Name = "flpAsientos";
            flpAsientos.Size = new Size(500, 100);
            flpAsientos.TabIndex = 18;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(33, 383);
            label9.Name = "label9";
            label9.Size = new Size(39, 15);
            label9.TabIndex = 19;
            label9.Text = "TOTAL";
            // 
            // frmReservaABM
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(572, 450);
            Controls.Add(label9);
            Controls.Add(flpAsientos);
            Controls.Add(cmbDestino);
            Controls.Add(cmbOrigen);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(btnNuevoPasajero);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(lblMensaje);
            Controls.Add(lblTarifa);
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
        private Button btnRefrescar;
        private Label label4;
        private Label label5;
        private Button btnNuevoPasajero;
        private Label label6;
        private Label label7;
        private Label label8;
        private ComboBox cmbOrigen;
        private ComboBox cmbDestino;
        private FlowLayoutPanel flpAsientos;
        private Label label9;
    }
}