namespace SkyPlus.Desktop
{
    partial class frmBoardingPass
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
        /// La pantalla se arma por código en frmBoardingPass.cs (ConstruirPantalla).
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            //
            // frmBoardingPass
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 650);
            Name = "frmBoardingPass";
            Text = "Boarding Pass";
            Load += frmBoardingPass_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
