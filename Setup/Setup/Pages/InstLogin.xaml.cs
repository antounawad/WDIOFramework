using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            TxtLogin.Text = SetupHelper.UserName;
            TxtPwd.Password = SetupHelper.Password;
            MainWindow.Instance.BtnNext.IsEnabled = !(String.IsNullOrEmpty(TxtLogin.Text) && String.IsNullOrEmpty(TxtPwd.Password));
        }
        public void OnLoadComplete()
        {
            if (SetupHelper.OfflineInstall)
            {
                MainWindow.Instance.NavigateToPage(new InstChoosePath());
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

            SetupHelper.UserName = TxtLogin.Text.Trim();
            SetupHelper.Password = TxtPwd?.Password?.Trim() ?? String.Empty;

            using (new Tools.WaitCursor())
            {
                switch (SetupHelper.DownloadManifest())
                {
                    case UpdateClient.EUpdateCheckResult.UpdatesAvailable:
                        break;

                    case UpdateClient.EUpdateCheckResult.AuthFail:
                        //MessageBox.Show("Zugangsdaten falsch!");
                        LblOnlinAuth.Text = "Zugangsdaten abgelehnt: Bitte überprüfen Sie Ihr Passwort!";
                        return;

                    case UpdateClient.EUpdateCheckResult.ClientIdFail:
                        LblOnlinAuth.Text = "Der EULG-Beratungsclient wurde bereits am " + SetupHelper.UpdateClient.ClientIdUsedDateTime + " auf dem Rechner " + SetupHelper.UpdateClient.ClientIdUsedHostname + " installiert." + Environment.NewLine
                                            + "Sie können den Client nur einmal installieren." + Environment.NewLine
                                            + "Falls Sie die Software auf diesem Rechner nutzen möchten, deinstallieren Sie die Software zuerst auf dem anderen System!" + Environment.NewLine
                                            + "Haben Sie die Deinstallation auf dem alten Rechner bereits durchgeführt " + Environment.NewLine
                                            + "und die Installation kann trotzdem nicht durchgeführt werden, so kontaktieren Sie bitte die EULG Software GmbH unter 0681 / 210738-0.";
                        return;

                    case UpdateClient.EUpdateCheckResult.InitialPasswordNotSet:
                        LblOnlinAuth.Text = "Ihr Passwort wurde noch nicht gesetzt." + Environment.NewLine
                                            + "Bitte öffnen Sie die E-Mail, die Sie während des Anmeldeprozesses erhalten haben, und legen Sie ein Passwort fest.";
                        return;

                    default:
                        MessageBox.Show(string.Join(Environment.NewLine, SetupHelper.UpdateClient.LogErrorMessages), "Fehler beim Download der Manifest-Daten!", MessageBoxButton.OK, MessageBoxImage.Error);
                        LblOnlinAuth.Text = "Fehler beim Download der Daten!";
                        return;
                }
            }

            MainWindow.Instance.NavigateToPage(new InstChoosePath());
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
