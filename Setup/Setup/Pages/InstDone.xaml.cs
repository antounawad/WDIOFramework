using System;
using System.Diagnostics;
using System.Windows.Controls;
using Eulg.Setup.Shared;
using Path = System.IO.Path;

namespace Eulg.Setup.Pages
{
    public partial class InstDone : UserControl, ISetupPageBase
    {
        public InstDone()
        {
            InitializeComponent();

            PageTitle = "Installation abgeschlossen";
            HasPrev = false;
            HasNext = true;
            NextButtonText = "Fertig";
        }

        public string PageTitle { get; set; }
        public Type PrevPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public string NextButtonText { get; set; }

        public void OnLoad()
        {
        }
        public void OnLoadComplete()
        {
            if (SetupHelper.OfflineInstall) OnNext();
        }
        public void OnNext()
        {
            SetupHelper.DelTree(Path.Combine(Path.GetTempPath(), Temp.WebInstTempFolder)); //TODO temp path names
            SetupHelper.DelTree(Path.Combine(Path.GetTempPath(), Temp.SetupTempFolder));

            if (ChkStartEulg.IsChecked ?? false)
            {
                var setup = App.Setup;
                var p = new Process
                {
                    StartInfo =
                    {
                        FileName = "Explorer.exe",
                        Arguments = Path.Combine(setup.InstallPath, setup.Branding.FileSystem.AppBinary)
                    }
                };
                p.Start();
                p.Dispose();
            }
            Environment.ExitCode = 1;
            MainWindow.Instance.Close();
        }
        public bool OnPrev()
        {
            return true;
        }
        public bool OnClose()
        {
            return true;
        }
    }
}
