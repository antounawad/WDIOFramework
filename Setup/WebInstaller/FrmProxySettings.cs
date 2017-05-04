using System;
using System.Globalization;
using System.Windows.Forms;
using Eulg.Shared;

namespace Eulg.Setup.WebInstaller
{
    public partial class FrmProxySettings : Form
    {
        public FrmProxySettings()
        {
            InitializeComponent();

            TxtAddress.Text = ProxyConfig.Instance.Address;
            TxtPort.Text = ProxyConfig.Instance.HttpPort.GetValueOrDefault(0).ToString(CultureInfo.InvariantCulture);
            TxtUserName.Text = ProxyConfig.Instance.Username;
            TxtPassword.Text = ProxyConfig.Instance.Password;
            TxtDomain.Text = ProxyConfig.Instance.Domain;

            switch (ProxyConfig.Instance.ProxyType)
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
                ProxyConfig.Instance.ProxyType = ProxyConfig.EProxyType.Manual;
            else if (rbProxyTypeNone.Checked)
                ProxyConfig.Instance.ProxyType = ProxyConfig.EProxyType.None;
            else
                ProxyConfig.Instance.ProxyType = ProxyConfig.EProxyType.Default;

            ProxyConfig.Instance.Address = TxtAddress.Text.Trim();
            ushort port;
            if (ushort.TryParse(TxtPort.Text.Trim(), out port))
            {
                ProxyConfig.Instance.HttpPort = port;
            }
            ProxyConfig.Instance.Username = TxtUserName.Text.Trim();
            ProxyConfig.Instance.Password = TxtPassword.Text.Trim();
            ProxyConfig.Instance.Domain = TxtDomain.Text.Trim();

            if (ProxyConfig.Instance.ProxyType == ProxyConfig.EProxyType.Manual &&
                (string.IsNullOrWhiteSpace(ProxyConfig.Instance.Address) || ProxyConfig.Instance.HttpPort < 1))
            {
                MessageBox.Show("Bitte Adresse und Port angeben!", "Hinweis", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            ProxyConfig.Instance.WriteToRegistry();
            ProxyConfig.Instance.SetDefault();

            Close();
        }

        private void rbProxyType_CheckedChanged(object sender, EventArgs e)
        {
            gbManual.Enabled = rbProxyTypeManual.Checked;
        }
    }
}