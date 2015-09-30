using System;
using System.Globalization;
using System.Windows.Forms;
using Eulg.Setup.Shared;

namespace Eulg.Setup.WebInstaller
{
    public partial class FrmProxySettings : Form
    {
        public FrmProxySettings()
        {
            InitializeComponent();

            TxtAddress.Text = ProxyConfig.Address;
            TxtPort.Text = ProxyConfig.HttpPort.ToString(CultureInfo.InvariantCulture);
            TxtUserName.Text = ProxyConfig.Username;
            TxtPassword.Text = ProxyConfig.Password;
            TxtDomain.Text = ProxyConfig.Domain;

            switch (ProxyConfig.ProxyType)
            {
                case ProxyConfig.EProxyType.Default:
                    rbProxyTypeDefault.Checked = true;
                    break;
                case ProxyConfig.EProxyType.None:
                    rbProxyTypeNone.Checked = true;
                    break;
                case ProxyConfig.EProxyType.Manual:
                    rbProxyTypeManual.Checked = true;
                    break;
            }

            rbProxyType_CheckedChanged(null, null);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (rbProxyTypeManual.Checked)
                ProxyConfig.ProxyType = ProxyConfig.EProxyType.Manual;
            else if (rbProxyTypeNone.Checked)
                ProxyConfig.ProxyType = ProxyConfig.EProxyType.None;
            else
                ProxyConfig.ProxyType = ProxyConfig.EProxyType.Default;

            ProxyConfig.Address = TxtAddress.Text.Trim();
            ushort port;
            if (ushort.TryParse(TxtPort.Text.Trim(), out port))
            {
                ProxyConfig.HttpPort = port;
            }
            ProxyConfig.Username = TxtUserName.Text.Trim();
            ProxyConfig.Password = TxtPassword.Text.Trim();
            ProxyConfig.Domain = TxtDomain.Text.Trim();

            if (ProxyConfig.ProxyType == ProxyConfig.EProxyType.Manual &&
                (string.IsNullOrWhiteSpace(ProxyConfig.Address) || ProxyConfig.HttpPort < 1))
            {
                MessageBox.Show("Bitte Adresse und Port angeben!", "Hinweis", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            ProxyConfig.WriteToRegistry();
            ProxyConfig.SetDefault();

            Close();
        }

        private void rbProxyType_CheckedChanged(object sender, EventArgs e)
        {
            gbManual.Enabled = rbProxyTypeManual.Checked;
        }
    }
}