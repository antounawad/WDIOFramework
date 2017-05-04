using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Eulg.Shared;

namespace Eulg.Setup.Pages
{
    public partial class MaintainUninstall : UserControl, ISetupPageBase
    {
        private readonly Profile _profile;

        public MaintainUninstall()
        {
            _profile = App.Setup.ReadInstalledProfile();

            InitializeComponent();

            PageTitle = $"{_profile?.DesktopApp ?? "Anwendung"} entfernen";
            HasPrev = false;
            HasNext = true;
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
            MainWindow.Instance.NavigateToPage(SetupHelper.ProgressPage);
            new Task(delegate
            {
                if (!DoUninstall())
                {
                    Dispatcher.Invoke(new Action(() => MainWindow.Instance.NavigateToPage(new Failed())));
                    return;
                }
                Dispatcher.Invoke(new Action(() => MainWindow.Instance.NavigateToPage(new MaintainDone())));
            }).Start();
        }
        public bool OnPrev()
        {
            return true;
        }
        public bool OnClose()
        {
            return true;
        }
        private bool DoUninstall()
        {
            var setup = App.Setup;

            SetupHelper.ReportProgress("Laufende Prozesse überprüfen...", "", -1);
            if (!setup.CheckRunningProcesses())
            {
                return false;
            }
            SetupHelper.ReportProgress("Update-Dienst stoppen...", "", -1);
            SetupHelper.StopService(true);
            SetupHelper.ReportProgress("Programmsymbole entfernen...", "", -1);
            if (!SetupHelper.RemoveShellIcons(_profile))
            {
                return false;
            }
            SetupHelper.ReportProgress("Einstellungen entfernen...", "", -1);
            if (!setup.ClearReg())
            {
                return false;
            }
            SetupHelper.ReportProgress("Native Images entfernen...", "", -1);
            if (!setup.UninstallNgen())
            {
                return false;
            }
            SetupHelper.ReportProgress("Programmdateien entfernen...", "", -1);
            if (!setup.ClearAppDir())
            {
                return false;
            }
            // ClearAppData
            SetupHelper.ReportProgress("Uninstaller entfernen...", "", -1);
            if (!setup.UnregisterUninstall())
            {
                return false;
            }

            var t = new Task(() =>
            {
                setup.UpdateClient.DeleteClientIdOnServer();
            });
            t.Start();
            t.Wait(10000);

            Thread.Sleep(500);
            return true;
        }
    }
}
