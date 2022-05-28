namespace FFXIV_RotationHelper
{
    partial class RotationWindow
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
            this.skillLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // skillLabel
            // 
            this.skillLabel.AutoSize = true;
            this.skillLabel.BackColor = System.Drawing.Color.LightYellow;
            this.skillLabel.Location = new System.Drawing.Point(634, 4);
            this.skillLabel.Name = "skillLabel";
            this.skillLabel.Size = new System.Drawing.Size(33, 12);
            this.skillLabel.TabIndex = 0;
            this.skillLabel.Text = "label1";
            // 
            // RotationWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Lime;
            this.ClientSize = new System.Drawing.Size(690, 57);
            this.ControlBox = false;
            this.Controls.Add(this.skillLabel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(34, 40);
            this.Name = "RotationWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "RotationWindow";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Lime;
            this.SizeChanged += new System.EventHandler(this.RotationWindow_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label skillLabel;
    }
}