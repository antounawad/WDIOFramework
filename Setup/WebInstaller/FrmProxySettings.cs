using System;
using System.Globalization;
using System.Net;
using System.Windows.Forms;
using Eulg.Setup.Shared;

namespace Eulg.Setup.WebInstaller
{
    public partial class FrmProxySettings : Form
    {
        public FrmProxySettings()
        {
            InitializeComponent();

            TxtAddress.Text = ProxyConfig.Instance.Address;
            TxtPort.Text = ProxyConfig.Instance.HttpPort.ToString(CultureInfo.InvariantCulture);
            TxtUserName.Text = ProxyConfig.Instance.Username;
            TxtPassword.Text = ProxyConfig.Instance.Password;
            TxtDomain.Text = ProxyConfig.Instance.Domain;
            chkUseSystem.Checked = (ProxyConfig.Instance.Address == null);

            if (string.IsNullOrEmpty(ProxyConfig.Instance.Address) && WebRequest.DefaultWebProxy != null)
            {
                var proxy = WebRequest.DefaultWebProxy.GetProxy(new Uri("https://service.eulg.de"));
                if (proxy != null && !proxy.Host.Equals("service.eulg.de", StringComparison.InvariantCultureIgnoreCase))
                {
                    TxtAddress.Text = proxy.Host;
                    TxtPort.Text = proxy.Port.ToString(CultureInfo.InvariantCulture);
                    var credentials = WebRequest.DefaultWebProxy.Credentials as NetworkCredential;
                    if (credentials != null)
                    {
                        TxtUserName.Text = credentials.UserName;
                        TxtPassword.Text = credentials.Password;
                        TxtDomain.Text = credentials.Domain;
                    }
                }
            }
            chkUseSystem_CheckedChanged(null, null);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (chkUseSystem.Checked)
            {
                ProxyConfig.Instance.Address = null;
            }
            else
            {
                ProxyConfig.Instance.Address = TxtAddress.Text.Trim();
                ushort port;
                ushort.TryParse(TxtPort.Text.Trim(), out port);
                ProxyConfig.Instance.HttpPort = port;
                ProxyConfig.Instance.Username = TxtUserName.Text.Trim();
                ProxyConfig.Instance.Password = TxtPassword.Text.Trim();
                ProxyConfig.Instance.Domain = TxtDomain.Text.Trim();
            }

            ProxyConfig.Instance.WriteToRegistry();
            ProxyConfig.Instance.SetDefault();

            Close();
        }

        private void chkUseSystem_CheckedChanged(object sender, EventArgs e)
        {
            TxtAddress.Enabled = TxtDomain.Enabled = TxtPort.Enabled = TxtUserName.Enabled = TxtPassword.Enabled = (!chkUseSystem.Checked);
        }
    }
}
