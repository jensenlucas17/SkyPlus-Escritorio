namespace SkyPlus.Desktop
{
    partial class frmVueloABM
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
            txtNumeroVuelo = new TextBox();
            label1 = new Label();
            cmbOrigen = new ComboBox();
            cmbDestino = new ComboBox();
            cmbAeronave = new ComboBox();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            dtpSalida = new DateTimePicker();
            dtpLlegada = new DateTimePicker();
            label5 = new Label();
            label6 = new Label();
            cmbEstado = new ComboBox();
            txtTarifa = new TextBox();
            label7 = new Label();
            label8 = new Label();
            btnGuardar = new Button();
            btnCancelar = new Button();
            lblMensaje = new Label();
            SuspendLayout();
            // 
            // txtNumeroVuelo
            // 
            txtNumeroVuelo.Location = new Point(297, 21);
            txtNumeroVuelo.Name = "txtNumeroVuelo";
            txtNumeroVuelo.Size = new Size(132, 23);
            txtNumeroVuelo.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(153, 21);
            label1.Name = "label1";
            label1.Size = new Size(102, 15);
            label1.TabIndex = 1;
            label1.Text = "Numero de vuelo:";
            // 
            // cmbOrigen
            // 
            cmbOrigen.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbOrigen.FormattingEnabled = true;
            cmbOrigen.Location = new Point(297, 67);
            cmbOrigen.Name = "cmbOrigen";
            cmbOrigen.Size = new Size(132, 23);
            cmbOrigen.TabIndex = 2;
            // 
            // cmbDestino
            // 
            cmbDestino.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDestino.FormattingEnabled = true;
            cmbDestino.Location = new Point(297, 118);
            cmbDestino.Name = "cmbDestino";
            cmbDestino.Size = new Size(132, 23);
            cmbDestino.TabIndex = 3;
            // 
            // cmbAeronave
            // 
            cmbAeronave.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAeronave.FormattingEnabled = true;
            cmbAeronave.Location = new Point(297, 170);
            cmbAeronave.Name = "cmbAeronave";
            cmbAeronave.Size = new Size(132, 23);
            cmbAeronave.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(153, 67);
            label2.Name = "label2";
            label2.Size = new Size(46, 15);
            label2.TabIndex = 5;
            label2.Text = "Origen:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(153, 118);
            label3.Name = "label3";
            label3.Size = new Size(50, 15);
            label3.TabIndex = 6;
            label3.Text = "Destino:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(153, 170);
            label4.Name = "label4";
            label4.Size = new Size(60, 15);
            label4.TabIndex = 7;
            label4.Text = "Aeronave:";
            // 
            // dtpSalida
            // 
            dtpSalida.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpSalida.Format = DateTimePickerFormat.Custom;
            dtpSalida.Location = new Point(295, 222);
            dtpSalida.Name = "dtpSalida";
            dtpSalida.ShowUpDown = true;
            dtpSalida.Size = new Size(200, 23);
            dtpSalida.TabIndex = 8;
            // 
            // dtpLlegada
            // 
            dtpLlegada.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpLlegada.Format = DateTimePickerFormat.Custom;
            dtpLlegada.Location = new Point(296, 268);
            dtpLlegada.Name = "dtpLlegada";
            dtpLlegada.ShowUpDown = true;
            dtpLlegada.Size = new Size(200, 23);
            dtpLlegada.TabIndex = 9;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(153, 222);
            label5.Name = "label5";
            label5.Size = new Size(41, 15);
            label5.TabIndex = 10;
            label5.Text = "Salida:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(153, 268);
            label6.Name = "label6";
            label6.Size = new Size(51, 15);
            label6.TabIndex = 11;
            label6.Text = "Llegada:";
            // 
            // cmbEstado
            // 
            cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEstado.FormattingEnabled = true;
            cmbEstado.Location = new Point(297, 316);
            cmbEstado.Name = "cmbEstado";
            cmbEstado.Size = new Size(132, 23);
            cmbEstado.TabIndex = 12;
            // 
            // txtTarifa
            // 
            txtTarifa.Location = new Point(297, 361);
            txtTarifa.Name = "txtTarifa";
            txtTarifa.Size = new Size(132, 23);
            txtTarifa.TabIndex = 13;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(153, 316);
            label7.Name = "label7";
            label7.Size = new Size(45, 15);
            label7.TabIndex = 14;
            label7.Text = "Estado:";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(153, 361);
            label8.Name = "label8";
            label8.Size = new Size(39, 15);
            label8.TabIndex = 15;
            label8.Text = "Tarifa:";
            // 
            // btnGuardar
            // 
            btnGuardar.Location = new Point(289, 405);
            btnGuardar.Name = "btnGuardar";
            btnGuardar.Size = new Size(75, 23);
            btnGuardar.TabIndex = 16;
            btnGuardar.Text = "Guardar";
            btnGuardar.UseVisualStyleBackColor = true;
            // 
            // btnCancelar
            // 
            btnCancelar.Location = new Point(420, 405);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(75, 23);
            btnCancelar.TabIndex = 17;
            btnCancelar.Text = "Cancelar";
            btnCancelar.UseVisualStyleBackColor = true;
            // 
            // lblMensaje
            // 
            lblMensaje.AutoSize = true;
            lblMensaje.ForeColor = Color.Red;
            lblMensaje.Location = new Point(296, 387);
            lblMensaje.Name = "lblMensaje";
            lblMensaje.Size = new Size(38, 15);
            lblMensaje.TabIndex = 18;
            lblMensaje.Text = "label9";
            lblMensaje.Visible = false;
            // 
            // frmVueloABM
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lblMensaje);
            Controls.Add(btnCancelar);
            Controls.Add(btnGuardar);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(txtTarifa);
            Controls.Add(cmbEstado);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(dtpLlegada);
            Controls.Add(dtpSalida);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(cmbAeronave);
            Controls.Add(cmbDestino);
            Controls.Add(cmbOrigen);
            Controls.Add(label1);
            Controls.Add(txtNumeroVuelo);
            Name = "frmVueloABM";
            Text = "Form1";
            Load += frmVueloABM_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtNumeroVuelo;
        private Label label1;
        private ComboBox cmbOrigen;
        private ComboBox cmbDestino;
        private ComboBox cmbAeronave;
        private Label label2;
        private Label label3;
        private Label label4;
        private DateTimePicker dtpSalida;
        private DateTimePicker dtpLlegada;
        private Label label5;
        private Label label6;
        private ComboBox cmbEstado;
        private TextBox txtTarifa;
        private Label label7;
        private Label label8;
        private Button btnGuardar;
        private Button btnCancelar;
        private Label lblMensaje;
    }
}