using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Eulg.Shared;

namespace Eulg.Setup.Pages
{
    public partial class InstLogin : UserControl, ISetupPageBase
    {
        public InstLogin()
        {
            InitializeComponent();

            PageTitle = "Anmeldung";
            HasNext = true;
            HasPrev = false;
            NextButtonText = "Anmelden";
        }

        public string PageTitle { get; set; }
        public Type PrevPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public string NextButtonText { get; set; }

        public void OnLoad()
        {
            if (!UpdateClient.CheckConnectivity())
            {
                MessageBox.Show("Keine Internetverbindung!");
                return;
            }

            var args = Environment.GetCommandLineArgs();
            TxtLogin.Text = args.Where(a => a.StartsWith("/A:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring(3)).FirstOrDefault() ?? "";
            TxtPwd.Password = args.Where(a => a.StartsWith("/P:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring(3)).FirstOrDefault() ?? "";

            MainWindow.Instance.BtnNext.IsEnabled = !(String.IsNullOrEmpty(TxtLogin.Text) && String.IsNullOrEmpty(TxtPwd.Password));
        }
        public void OnLoadComplete()
        {
            if (SetupHelper.OfflineInstall)
            {
                Profile profile;
                using (var stream = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Profile.xml"), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    profile = (Profile)new XmlSerializer(typeof(Profile)).Deserialize(stream);
                }

                MainWindow.Instance.NavigateToPage(new InstChoosePath(profile, SetupHelper.DefaultUsername ?? "", SetupHelper.DefaultPassword ?? ""));
            }
        }
        public void OnNext()
        {
            LblOnlinAuth.Text = "";

            if (String.IsNullOrWhiteSpace(TxtLogin.Text))
            {
                LblOnlinAuth.Text = "Bitte Benutzername (E-Mail-Adresse) und Passwort angeben!";
                return;
            }

            if (!UpdateClient.CheckConnectivity())
            {
                MessageBox.Show("Keine Internetverbindung!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var username = TxtLogin.Text.Trim();
            var password = TxtPwd?.Password?.Trim() ?? String.Empty;

            Profile profile;
            using (new Tools.WaitCursor())
            {
                var setup = App.Setup;
                var brandingApi = new BrandingApi(setup.Config.ApiManifestUri, setup.Config.Channel);
                var brandingInfo = brandingApi.GetProfile(username, password);
                if (!brandingInfo.Authenticated || !string.IsNullOrEmpty(brandingInfo.Message))
                {
                    LblOnlinAuth.Text = string.IsNullOrEmpty(brandingInfo.Message) ? "Zugangsdaten abgelehnt: Bitte überprüfen Sie Ihr Passwort!" : brandingInfo.Message;
                    return;
                }

                profile = brandingInfo.Profile;

                switch(setup.DownloadManifest(username, password))
                {
                    case UpdateClient.EUpdateCheckResult.UpdatesAvailable:
                        break;

                    case UpdateClient.EUpdateCheckResult.AuthFail:
                        //MessageBox.Show("Zugangsdaten falsch!");
                        LblOnlinAuth.Text = "Zugangsdaten abgelehnt: Bitte überprüfen Sie Ihr Passwort!";
                        return;

                    case UpdateClient.EUpdateCheckResult.ClientIdFail:
                        LblOnlinAuth.Text = "Der xbAV-Berater wurde bereits am " + setup.UpdateClient.ClientIdUsedDateTime + " auf dem Rechner " + setup.UpdateClient.ClientIdUsedHostname + " installiert." + Environment.NewLine
                                            + "Sie können den Client nur einmal installieren." + Environment.NewLine
                                            + "Falls Sie die Software auf diesem Rechner nutzen möchten, deinstallieren Sie die Software zuerst auf dem anderen System!" + Environment.NewLine
                                            + "Haben Sie die Deinstallation auf dem alten Rechner bereits durchgeführt " + Environment.NewLine
                                            + "und die Installation kann trotzdem nicht durchgeführt werden, so kontaktieren Sie bitte die xbAV Beratungssoftware GmbH unter 0681 / 210738-0.";
                        return;

                    case UpdateClient.EUpdateCheckResult.InitialPasswordNotSet:
                        LblOnlinAuth.Text = "Ihr Passwort wurde noch nicht gesetzt." + Environment.NewLine
                                            + "Bitte öffnen Sie die E-Mail, die Sie während des Anmeldeprozesses erhalten haben, und legen Sie ein Passwort fest.";
                        return;

                    default:
                        MessageBox.Show(string.Join(Environment.NewLine, setup.UpdateClient.LogErrorMessages), "Fehler beim Download der Manifest-Daten!", MessageBoxButton.OK, MessageBoxImage.Error);
                        LblOnlinAuth.Text = "Fehler beim Download der Daten!";
                        return;
                }
            }

            MainWindow.Instance.NavigateToPage(new InstChoosePath(profile, username, password));
        }
        public bool OnPrev()
        {
            return false;
        }
        public bool OnClose()
        {
            return true;
        }

        private void TxtLogin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TxtPwd.Focus();
            }
        }
        private void TxtPwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnNext();
            }
        }
        private void Txt_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow.Instance.BtnNext.IsEnabled = (!String.IsNullOrWhiteSpace(TxtLogin.Text) && !String.IsNullOrWhiteSpace(TxtPwd.Password));
        }
        private void TxtPwd_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.BtnNext.IsEnabled = (!String.IsNullOrWhiteSpace(TxtLogin.Text) && !String.IsNullOrWhiteSpace(TxtPwd.Password));
        }
    }
}
