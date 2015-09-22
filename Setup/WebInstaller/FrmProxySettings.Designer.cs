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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtUserName = new System.Windows.Forms.TextBox();
            this.TxtPassword = new System.Windows.Forms.TextBox();
            this.BtnOk = new System.Windows.Forms.Button();
            this.BtnAbort = new System.Windows.Forms.Button();
            this.TxtAddress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TxtDomain = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TxtPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkUseSystem = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "&Benutzername:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(42, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "&Kennwort:";
            // 
            // TxtUserName
            // 
            this.TxtUserName.Location = new System.Drawing.Point(103, 58);
            this.TxtUserName.Name = "TxtUserName";
            this.TxtUserName.Size = new System.Drawing.Size(227, 20);
            this.TxtUserName.TabIndex = 6;
            // 
            // TxtPassword
            // 
            this.TxtPassword.Location = new System.Drawing.Point(103, 84);
            this.TxtPassword.Name = "TxtPassword";
            this.TxtPassword.Size = new System.Drawing.Size(227, 20);
            this.TxtPassword.TabIndex = 8;
            this.TxtPassword.UseSystemPasswordChar = true;
            // 
            // BtnOk
            // 
            this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOk.Location = new System.Drawing.Point(196, 166);
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
            this.BtnAbort.Location = new System.Drawing.Point(277, 166);
            this.BtnAbort.Name = "BtnAbort";
            this.BtnAbort.Size = new System.Drawing.Size(75, 23);
            this.BtnAbort.TabIndex = 12;
            this.BtnAbort.Text = "Abbrechen";
            this.BtnAbort.UseVisualStyleBackColor = true;
            // 
            // TxtAddress
            // 
            this.TxtAddress.Location = new System.Drawing.Point(103, 32);
            this.TxtAddress.Name = "TxtAddress";
            this.TxtAddress.Size = new System.Drawing.Size(141, 20);
            this.TxtAddress.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(49, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "&Adresse:";
            // 
            // TxtDomain
            // 
            this.TxtDomain.Location = new System.Drawing.Point(103, 110);
            this.TxtDomain.Name = "TxtDomain";
            this.TxtDomain.Size = new System.Drawing.Size(227, 20);
            this.TxtDomain.TabIndex = 10;
            this.TxtDomain.UseSystemPasswordChar = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 113);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "&Domäne:";
            // 
            // TxtPort
            // 
            this.TxtPort.Location = new System.Drawing.Point(285, 32);
            this.TxtPort.MaxLength = 5;
            this.TxtPort.Name = "TxtPort";
            this.TxtPort.Size = new System.Drawing.Size(45, 20);
            this.TxtPort.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(250, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "&Port:";
            // 
            // chkUseSystem
            // 
            this.chkUseSystem.AutoSize = true;
            this.chkUseSystem.Location = new System.Drawing.Point(103, 9);
            this.chkUseSystem.Name = "chkUseSystem";
            this.chkUseSystem.Size = new System.Drawing.Size(182, 17);
            this.chkUseSystem.TabIndex = 0;
            this.chkUseSystem.Text = "System-Einstellungen verwenden";
            this.chkUseSystem.UseVisualStyleBackColor = true;
            this.chkUseSystem.CheckedChanged += new System.EventHandler(this.chkUseSystem_CheckedChanged);
            // 
            // FrmProxySettings
            // 
            this.AcceptButton = this.BtnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnAbort;
            this.ClientSize = new System.Drawing.Size(364, 201);
            this.Controls.Add(this.chkUseSystem);
            this.Controls.Add(this.TxtPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TxtDomain);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.TxtAddress);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.BtnAbort);
            this.Controls.Add(this.BtnOk);
            this.Controls.Add(this.TxtPassword);
            this.Controls.Add(this.TxtUserName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmProxySettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Proxy-Server konfigurieren";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtUserName;
        private System.Windows.Forms.TextBox TxtPassword;
        private System.Windows.Forms.Button BtnOk;
        private System.Windows.Forms.Button BtnAbort;
        private System.Windows.Forms.TextBox TxtAddress;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TxtDomain;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TxtPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkUseSystem;
    }
}