namespace SkyPlus.Desktop
{
    partial class frmCheckin
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
        /// La pantalla se arma por código en frmCheckin.cs (ConstruirPantalla).
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            //
            // frmCheckin
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 650);
            Name = "frmCheckin";
            Text = "Check-in";
            Load += frmCheckin_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
