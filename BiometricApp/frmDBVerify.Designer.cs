namespace BiometricApp
{
    partial class frmDBVerify
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
            this.cboReaders = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.alertLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pbFingerprint = new System.Windows.Forms.PictureBox();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFingerprint)).BeginInit();
            this.SuspendLayout();
            // 
            // cboReaders
            // 
            this.cboReaders.Font = new System.Drawing.Font("Tahoma", 8F);
            this.cboReaders.Location = new System.Drawing.Point(225, 68);
            this.cboReaders.Name = "cboReaders";
            this.cboReaders.Size = new System.Drawing.Size(159, 21);
            this.cboReaders.TabIndex = 21;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.alertLabel);
            this.panel2.Location = new System.Drawing.Point(225, 147);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(436, 90);
            this.panel2.TabIndex = 44;
            // 
            // alertLabel
            // 
            this.alertLabel.AutoSize = true;
            this.alertLabel.Font = new System.Drawing.Font("Poppins ExtraBold", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.alertLabel.Location = new System.Drawing.Point(58, 27);
            this.alertLabel.Name = "alertLabel";
            this.alertLabel.Size = new System.Drawing.Size(327, 34);
            this.alertLabel.TabIndex = 0;
            this.alertLabel.Text = "Place the Finger on the Scanner";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(61)))), ((int)(((byte)(115)))));
            this.pictureBox1.Location = new System.Drawing.Point(-1, -1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(737, 44);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 39;
            this.pictureBox1.TabStop = false;
            // 
            // pbFingerprint
            // 
            this.pbFingerprint.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(209)))), ((int)(((byte)(209)))));
            this.pbFingerprint.Location = new System.Drawing.Point(34, 68);
            this.pbFingerprint.Name = "pbFingerprint";
            this.pbFingerprint.Size = new System.Drawing.Size(159, 169);
            this.pbFingerprint.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbFingerprint.TabIndex = 19;
            this.pbFingerprint.TabStop = false;
            // 
            // frmDBVerify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(735, 253);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.cboReaders);
            this.Controls.Add(this.pbFingerprint);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDBVerify";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Verification";
            this.Load += new System.EventHandler(this.frmDBVerify_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbFingerprint)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.ComboBox cboReaders;
        internal System.Windows.Forms.PictureBox pbFingerprint;
        internal System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label alertLabel;
    }
}