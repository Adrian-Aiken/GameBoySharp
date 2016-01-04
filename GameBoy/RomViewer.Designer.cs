namespace GameBoy
{
    partial class RomViewer
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
            this.romTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // romTextBox
            // 
            this.romTextBox.AcceptsReturn = true;
            this.romTextBox.AcceptsTab = true;
            this.romTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.romTextBox.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.romTextBox.Location = new System.Drawing.Point(0, 0);
            this.romTextBox.MaxLength = 99999999;
            this.romTextBox.Multiline = true;
            this.romTextBox.Name = "romTextBox";
            this.romTextBox.ReadOnly = true;
            this.romTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.romTextBox.Size = new System.Drawing.Size(855, 527);
            this.romTextBox.TabIndex = 0;
            this.romTextBox.WordWrap = false;
            // 
            // RomViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 527);
            this.Controls.Add(this.romTextBox);
            this.Name = "RomViewer";
            this.Text = "RomViewer";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox romTextBox;
    }
}