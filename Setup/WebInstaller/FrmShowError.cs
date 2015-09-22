using System;
using System.Windows.Forms;

namespace Eulg.Setup.WebInstaller
{
    public partial class FrmShowError : Form
    {
        public FrmShowError()
        {
            InitializeComponent();
        }

        private void BtnRetry_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void BtnProxySettings_Click(object sender, EventArgs e)
        {
            using(var frm = new FrmProxySettings())
            {
                frm.ShowDialog();
            }
        }

    }
}
