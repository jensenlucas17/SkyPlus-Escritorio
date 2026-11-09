namespace SkyPlus.Desktop
{
    partial class frmReservas
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
        /// La pantalla se arma por código en frmReservas.cs (ConstruirPantalla).
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            //
            // frmReservas
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 650);
            Name = "frmReservas";
            Text = "Reservas";
            Load += frmReservas_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
