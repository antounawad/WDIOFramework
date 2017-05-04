using System.Windows.Controls;

namespace Eulg.Client.SupportTool.Views
{
    public partial class Progress : UserControl
    {
        public Progress()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int percent, string message, string header)
        {
            if (percent < 0)
            {
                ProgressBar.IsIndeterminate = true;
            }
            else
            {
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = percent;
            }
            LabelFileCount.Text = message;
            if (!string.IsNullOrEmpty(header)) LabelProgress.Text = header;
        }

    }
}
