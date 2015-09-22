namespace Eulg.Setup.WebInstaller
{
    partial class FrmShowError
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
            this.TxtMessage = new System.Windows.Forms.TextBox();
            this.BtnRetry = new System.Windows.Forms.Button();
            this.BtnAbort = new System.Windows.Forms.Button();
            this.BtnProxySettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TxtMessage
            // 
            this.TxtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtMessage.Location = new System.Drawing.Point(12, 12);
            this.TxtMessage.Multiline = true;
            this.TxtMessage.Name = "TxtMessage";
            this.TxtMessage.ReadOnly = true;
            this.TxtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TxtMessage.Size = new System.Drawing.Size(460, 208);
            this.TxtMessage.TabIndex = 0;
            this.TxtMessage.TabStop = false;
            // 
            // BtnRetry
            // 
            this.BtnRetry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnRetry.Location = new System.Drawing.Point(316, 226);
            this.BtnRetry.Name = "BtnRetry";
            this.BtnRetry.Size = new System.Drawing.Size(75, 23);
            this.BtnRetry.TabIndex = 2;
            this.BtnRetry.Text = "Wiederholen";
            this.BtnRetry.UseVisualStyleBackColor = true;
            this.BtnRetry.Click += new System.EventHandler(this.BtnRetry_Click);
            // 
            // BtnAbort
            // 
            this.BtnAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnAbort.Location = new System.Drawing.Point(397, 226);
            this.BtnAbort.Name = "BtnAbort";
            this.BtnAbort.Size = new System.Drawing.Size(75, 23);
            this.BtnAbort.TabIndex = 3;
            this.BtnAbort.Text = "Abbrechen";
            this.BtnAbort.UseVisualStyleBackColor = true;
            // 
            // BtnProxySettings
            // 
            this.BtnProxySettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnProxySettings.Location = new System.Drawing.Point(12, 226);
            this.BtnProxySettings.Name = "BtnProxySettings";
            this.BtnProxySettings.Size = new System.Drawing.Size(125, 23);
            this.BtnProxySettings.TabIndex = 1;
            this.BtnProxySettings.Text = "Proxy-Einstellungen";
            this.BtnProxySettings.UseVisualStyleBackColor = true;
            this.BtnProxySettings.Click += new System.EventHandler(this.BtnProxySettings_Click);
            // 
            // FrmShowError
            // 
            this.AcceptButton = this.BtnRetry;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnAbort;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.BtnProxySettings);
            this.Controls.Add(this.BtnAbort);
            this.Controls.Add(this.BtnRetry);
            this.Controls.Add(this.TxtMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmShowError";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fehler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox TxtMessage;
        public System.Windows.Forms.Button BtnRetry;
        public System.Windows.Forms.Button BtnAbort;
        public System.Windows.Forms.Button BtnProxySettings;
    }
}