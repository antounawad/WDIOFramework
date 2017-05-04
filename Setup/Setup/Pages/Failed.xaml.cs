using System;
using System.Windows;
using System.Windows.Controls;

namespace Eulg.Setup.Pages
{
    public partial class Failed : UserControl, ISetupPageBase
    {
        public Failed()
        {
            InitializeComponent();

            PageTitle = "Setup fehlgeschlagen!";
            NextButtonText = "Schließen";
            HasNext = true;
            HasPrev = false;
        }

        public string PageTitle { get; set; }
        public Type PrevPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public string NextButtonText { get; set; }

        public void OnLoad()
        {
            var tmp = String.Empty;
            if (App.Setup.UpdateClient != null)
            {
                foreach (var logMessage in App.Setup.UpdateClient.LogErrorMessages)
                {
                    tmp += logMessage + Environment.NewLine;
                }
            }
            TextBlock.Text = tmp;
            Environment.ExitCode = 2;
            
        }
        public void OnLoadComplete()
        {
            if (SetupHelper.OfflineInstall) OnNext();
        }
        public void OnNext()
        {
            MainWindow.Instance.Close();
        }
        public bool OnPrev()
        {
            return true;
        }
        public bool OnClose()
        {
            return false;
        }

    }
}
