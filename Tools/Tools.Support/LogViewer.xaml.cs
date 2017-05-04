using System.Windows;

namespace Eulg.Client.SupportTool
{
    public partial class LogViewer : Window
    {
        public LogViewer()
        {
            InitializeComponent();
        }

        public void SetTextClient(string text)
        {
            TextBoxClient.Text = text;
        }

        public void SetTextSync(string text)
        {
            TextBoxSync.Text = text;
        }
    }
}
