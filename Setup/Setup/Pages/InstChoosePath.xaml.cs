using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Eulg.Shared;

namespace Eulg.Setup.Pages
{
    public partial class InstChoosePath : ISetupPageBase
    {
        private readonly Profile _profile;
        private readonly string _username;
        private readonly string _password;

        public InstChoosePath(Profile profile, string username, string password)
        {
            _profile = profile;
            _username = username;
            _password = password;
            InitializeComponent();

            PageTitle = "Installations-Verzeichnis";
            HasPrev = true;
            HasNext = true;
        }

        public string PageTitle { get; set; }
        public Type PrevPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public string NextButtonText { get; set; }

        public void OnLoad()
        {
            TxtPath.Text = App.Setup.InstallPath;
            CheckDrive();
        }
        public void OnLoadComplete()
        {
            if (SetupHelper.OfflineInstall) OnNext();
        }
        public void OnNext()
        {
            if (!TxtPath.Text.Equals(Path.GetFullPath(TxtPath.Text), StringComparison.CurrentCultureIgnoreCase))
            {
                TxtPath.Text = Path.GetFullPath(TxtPath.Text);
                return;
            }
            if (!CheckDrive(true))
            {
                return;
            }
            var path = TxtPath.Text;
            path = Path.GetFullPath(path);
            App.Setup.InstallPath = path;
            MainWindow.Instance.NavigateToPage(SetupHelper.ProgressPage);
            new Task(delegate
            {
                if (!DoInstall())
                {
                    Dispatcher.Invoke(new Action(() => MainWindow.Instance.NavigateToPage(new Failed())));
                    return;
                }
                Dispatcher.Invoke(new Action(() => MainWindow.Instance.NavigateToPage(new InstDone())));
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

        private bool CheckDrive(bool showError = false)
        {
            try
            {
                var drive = TxtPath.Text.Length > 0 ? TxtPath.Text.Substring(0, 1) : "C";
                var driveInfo = new DriveInfo(drive);
                var spaceNeeded = App.Setup.UpdateClient.UpdateConf.UpdateFiles.Sum(s => s.FileSize);
                var spaceAvailable = driveInfo.AvailableFreeSpace;
                var spaceRemaining = spaceAvailable - spaceNeeded;
                LabelSpaceNeeded.Content = $"{decimal.Round(spaceNeeded / 1024m / 1024m):N0} MB";
                LabelSpaceAvailabel.Content = $"{decimal.Round(spaceAvailable / 1024m / 1024m):N0} MB";
                LabelSpaceRemaining.Content = $"{decimal.Round(spaceRemaining / 1024m / 1024m):N0} MB";
                return (spaceRemaining > 0 && driveInfo.IsReady && driveInfo.DriveType == DriveType.Fixed);
            }
            catch (Exception exception)
            {
                if (showError)
                {
                    MessageBox.Show(exception.GetMessagesTree(), "Hinweis", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }

        private bool DoInstall()
        {
            var setup = App.Setup;

            SetupHelper.CancelRequested = false;
            setup.UpdateClient.Log(UpdateClient.ELogTypeEnum.Info, $"Setup {setup.Version} ({setup.Branding.Info.BuildTag} / {setup.Config.ApiManifestUri})");

            SetupHelper.ReportProgress("Laufende Prozesse überprüfen...", "", -1);
            if (!setup.CheckRunningProcesses())
            {
                return false;
            }
            SetupHelper.ReportProgress("Update-Dienst entfernen...", "", -1);
            if (!setup.RemoveUpdateService())
            {
                return false;
            }

            long estimatedInstallSize;
            SetupHelper.ReportProgress("Programmdateien installieren...", "", -1);
            if (SetupHelper.OfflineInstall)
            {
                estimatedInstallSize = new FileInfo(SetupHelper.OfflineZipFile).Length / 1024;
                setup.ExtractAppDir();
            }
            else
            {
                while (true)
                {
                    if (setup.DownloadAppDir(true))
                    {
                        break;
                    }
                    if (MessageBox.Show("Beim Download der Programmdateien ist ein Fehler aufgetreten. Bitte überprüfen Sie Ihre Internet-Verbindung." + Environment.NewLine + Environment.NewLine + "Nochmal versuchen?", "Fehler", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel)
                    {
                        setup.Log(UpdateClient.ELogTypeEnum.Warning, "Setup wurde vom Benutzer abgebrochen!");
                        return false;
                    }
                    SetupHelper.CancelRequested = false;
                }
                estimatedInstallSize = setup.UpdateClient.UpdateConf.UpdateFiles.Sum(s => s.FileSize) / 1024;
            }
            SetupHelper.ReportProgress("Update-Dienst installieren...", "", -1);
            if (!setup.InstallUpdateService())
            {
                return false;
            }
            SetupHelper.ReportProgress("Uninstaller installieren...", "", -1);
            if (!setup.InstallUninstaller())
            {
                return false;
            }
            SetupHelper.ReportProgress("Uninstaller registrieren...", "", -1);
            if (!setup.RegisterUninstaller(_profile, estimatedInstallSize))
            {
                return false;
            }
            SetupHelper.ReportProgress("Einstellungen speichern (Registry)...", "", -1);
            if (!setup.PrepareRegistry(_profile, _username, _password))
            {
                return false;
            }
            SetupHelper.ReportProgress("Einstellungen speichern (Branding)...", "", -1);
            if (!setup.WriteBranding())
            {
                return false;
            }
            SetupHelper.ReportProgress("Programmstart beschleunigen...", "", -1);
            if (!setup.InstallNgen())
            {
                return false;
            }
            SetupHelper.ReportProgress("Protokoll versenden...", "", -1);
            if (!setup.UpdateClient.UploadLogfile())
            {
                if (!SetupHelper.OfflineInstall)
                {
                    return false;
                }
            }
            SetupHelper.ReportProgress("Programm-Symbole hinzufügen...", "", -1);
            Uri webverwaltung;
            var webverwaltungUrl = setup.TryGetApi(EApiResource.WebAdministration, out webverwaltung, true) ? webverwaltung.AbsoluteUri : null;
            if (!setup.AddShellIcons(_profile, webverwaltungUrl))
            {
                return false;
            }
            return true;
        }

        private void BtnChoosePath_OnClick(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.SelectedPath = TxtPath.Text;
                fbd.ShowNewFolderButton = true;
                fbd.ShowDialog();
                TxtPath.Text = fbd.SelectedPath;
            }
        }

        private void TxtPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckDrive();
        }

        private void TxtPath_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnNext();
            }
        }
    }
}
