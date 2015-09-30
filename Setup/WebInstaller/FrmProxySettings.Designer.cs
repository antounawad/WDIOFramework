namespace Eulg.Setup.WebInstaller
{
    partial class FrmProxySettings
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
            this.BtnOk = new System.Windows.Forms.Button();
            this.BtnAbort = new System.Windows.Forms.Button();
            this.rbProxyTypeDefault = new System.Windows.Forms.RadioButton();
            this.rbProxyTypeNone = new System.Windows.Forms.RadioButton();
            this.rbProxyTypeManual = new System.Windows.Forms.RadioButton();
            this.gbManual = new System.Windows.Forms.GroupBox();
            this.TxtPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.TxtDomain = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TxtAddress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TxtPassword = new System.Windows.Forms.TextBox();
            this.TxtUserName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.gbManual.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnOk
            // 
            this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOk.Location = new System.Drawing.Point(208, 224);
            this.BtnOk.Name = "BtnOk";
            this.BtnOk.Size = new System.Drawing.Size(75, 23);
            this.BtnOk.TabIndex = 11;
            this.BtnOk.Text = "&OK";
            this.BtnOk.UseVisualStyleBackColor = true;
            this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // BtnAbort
            // 
            this.BtnAbort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnAbort.Location = new System.Drawing.Point(289, 224);
            this.BtnAbort.Name = "BtnAbort";
            this.BtnAbort.Size = new System.Drawing.Size(75, 23);
            this.BtnAbort.TabIndex = 12;
            this.BtnAbort.Text = "Abbrechen";
            this.BtnAbort.UseVisualStyleBackColor = true;
            // 
            // rbProxyTypeDefault
            // 
            this.rbProxyTypeDefault.AutoSize = true;
            this.rbProxyTypeDefault.Location = new System.Drawing.Point(12, 12);
            this.rbProxyTypeDefault.Name = "rbProxyTypeDefault";
            this.rbProxyTypeDefault.Size = new System.Drawing.Size(235, 17);
            this.rbProxyTypeDefault.TabIndex = 0;
            this.rbProxyTypeDefault.TabStop = true;
            this.rbProxyTypeDefault.Text = "Proxy-Einstellungen des &Systems verwenden";
            this.rbProxyTypeDefault.UseVisualStyleBackColor = true;
            this.rbProxyTypeDefault.CheckedChanged += new System.EventHandler(this.rbProxyType_CheckedChanged);
            // 
            // rbProxyTypeNone
            // 
            this.rbProxyTypeNone.AutoSize = true;
            this.rbProxyTypeNone.Location = new System.Drawing.Point(12, 35);
            this.rbProxyTypeNone.Name = "rbProxyTypeNone";
            this.rbProxyTypeNone.Size = new System.Drawing.Size(75, 17);
            this.rbProxyTypeNone.TabIndex = 1;
            this.rbProxyTypeNone.TabStop = true;
            this.rbProxyTypeNone.Text = "&Kein Proxy";
            this.rbProxyTypeNone.UseVisualStyleBackColor = true;
            this.rbProxyTypeNone.CheckedChanged += new System.EventHandler(this.rbProxyType_CheckedChanged);
            // 
            // rbProxyTypeManual
            // 
            this.rbProxyTypeManual.AutoSize = true;
            this.rbProxyTypeManual.Location = new System.Drawing.Point(12, 58);
            this.rbProxyTypeManual.Name = "rbProxyTypeManual";
            this.rbProxyTypeManual.Size = new System.Drawing.Size(162, 17);
            this.rbProxyTypeManual.TabIndex = 2;
            this.rbProxyTypeManual.TabStop = true;
            this.rbProxyTypeManual.Text = "&Manuelle Proxy-Konfiguration";
            this.rbProxyTypeManual.UseVisualStyleBackColor = true;
            this.rbProxyTypeManual.CheckedChanged += new System.EventHandler(this.rbProxyType_CheckedChanged);
            // 
            // gbManual
            // 
            this.gbManual.Controls.Add(this.TxtPort);
            this.gbManual.Controls.Add(this.label3);
            this.gbManual.Controls.Add(this.TxtDomain);
            this.gbManual.Controls.Add(this.label5);
            this.gbManual.Controls.Add(this.TxtAddress);
            this.gbManual.Controls.Add(this.label4);
            this.gbManual.Controls.Add(this.TxtPassword);
            this.gbManual.Controls.Add(this.TxtUserName);
            this.gbManual.Controls.Add(this.label2);
            this.gbManual.Controls.Add(this.label1);
            this.gbManual.Location = new System.Drawing.Point(12, 81);
            this.gbManual.Name = "gbManual";
            this.gbManual.Size = new System.Drawing.Size(351, 137);
            this.gbManual.TabIndex = 3;
            this.gbManual.TabStop = false;
            // 
            // TxtPort
            // 
            this.TxtPort.Location = new System.Drawing.Point(278, 19);
            this.TxtPort.MaxLength = 5;
            this.TxtPort.Name = "TxtPort";
            this.TxtPort.Size = new System.Drawing.Size(45, 20);
            this.TxtPort.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(243, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "&Port:";
            // 
            // TxtDomain
            // 
            this.TxtDomain.Location = new System.Drawing.Point(96, 97);
            this.TxtDomain.Name = "TxtDomain";
            this.TxtDomain.Size = new System.Drawing.Size(227, 20);
            this.TxtDomain.TabIndex = 20;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(40, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "&Domäne:";
            // 
            // TxtAddress
            // 
            this.TxtAddress.Location = new System.Drawing.Point(96, 19);
            this.TxtAddress.Name = "TxtAddress";
            this.TxtAddress.Size = new System.Drawing.Size(141, 20);
            this.TxtAddress.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(42, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "&Adresse:";
            // 
            // TxtPassword
            // 
            this.TxtPassword.Location = new System.Drawing.Point(96, 71);
            this.TxtPassword.Name = "TxtPassword";
            this.TxtPassword.Size = new System.Drawing.Size(227, 20);
            this.TxtPassword.TabIndex = 18;
            this.TxtPassword.UseSystemPasswordChar = true;
            // 
            // TxtUserName
            // 
            this.TxtUserName.Location = new System.Drawing.Point(96, 45);
            this.TxtUserName.Name = "TxtUserName";
            this.TxtUserName.Size = new System.Drawing.Size(227, 20);
            this.TxtUserName.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 74);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "&Kennwort:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "&Benutzername:";
            // 
            // FrmProxySettings
            // 
            this.AcceptButton = this.BtnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnAbort;
            this.ClientSize = new System.Drawing.Size(376, 259);
            this.Controls.Add(this.gbManual);
            this.Controls.Add(this.rbProxyTypeManual);
            this.Controls.Add(this.rbProxyTypeNone);
            this.Controls.Add(this.rbProxyTypeDefault);
            this.Controls.Add(this.BtnAbort);
            this.Controls.Add(this.BtnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmProxySettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Proxy-Server konfigurieren";
            this.gbManual.ResumeLayout(false);
            this.gbManual.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button BtnOk;
        private System.Windows.Forms.Button BtnAbort;
        private System.Windows.Forms.RadioButton rbProxyTypeDefault;
        private System.Windows.Forms.RadioButton rbProxyTypeNone;
        private System.Windows.Forms.RadioButton rbProxyTypeManual;
        private System.Windows.Forms.GroupBox gbManual;
        private System.Windows.Forms.TextBox TxtPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TxtDomain;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TxtAddress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TxtPassword;
        private System.Windows.Forms.TextBox TxtUserName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}