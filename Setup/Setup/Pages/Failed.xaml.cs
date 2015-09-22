using System;
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
            if (SetupHelper.UpdateClient != null)
            {
                foreach (var logMessage in SetupHelper.UpdateClient.LogErrorMessages)
                {
                    tmp += logMessage + Environment.NewLine;
                }
            }
            TextBlock.Text = tmp;
            SetupHelper.ExitCode = 2;
            
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
