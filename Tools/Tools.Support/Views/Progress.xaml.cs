using System.Windows.Controls;

namespace Eulg.Client.SupportTool.Views
{
    public partial class Progress : UserControl
    {
        public Progress()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int percent, string message)
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
            LabelFileCount.Content = message;
        }

    }
}
