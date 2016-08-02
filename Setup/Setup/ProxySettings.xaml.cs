using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Eulg.Setup
{
    public partial class ProxySettings
    {
        private readonly ICollection<ValidationError> _errors;

        private string _proxyHost;
        private int _proxyHttpPort;
        private string _proxyUser;
        private string _proxyPass;
        private string _proxyDomain;

        public class ProxyConfigWrapper
        {
            public string Address { get { return Eulg.Shared.ProxyConfig.Instance.Address; } set { Eulg.Shared.ProxyConfig.Instance.Address = value; } }
            public ushort HttpPort { get { return Eulg.Shared.ProxyConfig.Instance.HttpPort.GetValueOrDefault(0); } set { Eulg.Shared.ProxyConfig.Instance.HttpPort = value; } }
            public string Username { get { return Eulg.Shared.ProxyConfig.Instance.Username; } set { Eulg.Shared.ProxyConfig.Instance.Username = value; } }
            public string Domain { get { return Eulg.Shared.ProxyConfig.Instance.Domain; } set { Eulg.Shared.ProxyConfig.Instance.Domain = value; } }
            public string Password { get { return Eulg.Shared.ProxyConfig.Instance.Password; } set { Eulg.Shared.ProxyConfig.Instance.Password = value; } }
            public bool ProxyTypeDefault { get { return (Eulg.Shared.ProxyConfig.Instance.ProxyType == Eulg.Shared.ProxyConfig.EProxyType.Default); } set { if (value) Eulg.Shared.ProxyConfig.Instance.ProxyType = Eulg.Shared.ProxyConfig.EProxyType.Default; } }
            public bool ProxyTypeNone { get { return (Eulg.Shared.ProxyConfig.Instance.ProxyType == Eulg.Shared.ProxyConfig.EProxyType.None); } set { if (value) Eulg.Shared.ProxyConfig.Instance.ProxyType = Eulg.Shared.ProxyConfig.EProxyType.None; } }
            public bool ProxyTypeManual { get { return (Eulg.Shared.ProxyConfig.Instance.ProxyType == Eulg.Shared.ProxyConfig.EProxyType.Manual); } set { if (value) Eulg.Shared.ProxyConfig.Instance.ProxyType = Eulg.Shared.ProxyConfig.EProxyType.Manual; } }
        }

        public ProxyConfigWrapper ProxyConfig { get; } = new ProxyConfigWrapper();



        public ProxySettings()
        {
            _errors = new HashSet<ValidationError>();

            RememberCurrentConfig();
            InitializeComponent();

            ClearPasswordButton.Visibility = string.IsNullOrEmpty(ProxyConfig.Password) ? Visibility.Collapsed : Visibility.Visible;
        }

        public static void ShowProxySettings()
        {
            var configControl = new ProxySettings();

            MainWindow.Instance.ShowCustomDialog(configControl, HorizontalAlignment.Right,
                Tuple.Create<string, Func<bool>>("Speichern", configControl.OnSaveProxySettings_Click),
                Tuple.Create<string, Func<bool>>("Schließen", configControl.OnLeaveProxySettings_Click));
        }

        private bool OnSaveProxySettings_Click()
        {
            if (_errors.Any())
            {
                if (string.IsNullOrEmpty(TextBoxHttpPort.Text))
                    TextBoxHttpPort.Text = "0";
                MessageBox.Show(MainWindow.Instance, "Ein oder mehrere Felder enthalten ungültige Eingaben. Bitte überprüfen Sie Ihre Einstellungen.",
                                "xbAV-Berater-Setup", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            try
            {
                Eulg.Shared.ProxyConfig.Instance.WriteToRegistry();
                Eulg.Shared.ProxyConfig.Instance.SetDefault();
                RememberCurrentConfig();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(MainWindow.Instance, "Leider wurden Ihre Einstellungen nicht erfolgreich gespeichert. Systemmeldung:" +
                    Environment.NewLine + Environment.NewLine +
                    ex.GetType().Name + ": " + ex.Message, "xbAV-Berater-Setup", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
        }

        private bool OnLeaveProxySettings_Click()
        {
            if (_proxyHost != ProxyConfig.Address
                || _proxyHttpPort != ProxyConfig.HttpPort
                || _proxyUser != ProxyConfig.Username
                || _proxyPass != ProxyConfig.Password
                || _proxyDomain != ProxyConfig.Domain)
            {
                return MessageBox.Show(MainWindow.Instance, "Ihre geänderten Einstellungen wurden noch nicht gespeichert! Möchten Sie die Einstellungen wirklich verwerfen?",
                                       "xbAV-Berater-Setup", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }

            return true;
        }

        private void RememberCurrentConfig()
        {
            _proxyHost = ProxyConfig.Address;
            _proxyHttpPort = ProxyConfig.HttpPort;
            _proxyUser = ProxyConfig.Username;
            _proxyPass = ProxyConfig.Password;
            _proxyDomain = ProxyConfig.Domain;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(sender is PasswordBox);
            ProxyConfig.Password = ((PasswordBox)sender).Password;

            ClearPasswordButton.Visibility = string.IsNullOrEmpty(ProxyConfig.Password) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnValidationStateChanged(object sender, ValidationErrorEventArgs e)
        {
            switch (e.Action)
            {
                case ValidationErrorEventAction.Added:
                    _errors.Add(e.Error);
                    break;
                case ValidationErrorEventAction.Removed:
                    _errors.Remove(e.Error);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void OnClearPassword(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = string.Empty;
            ProxyConfig.Password = string.Empty;
            ClearPasswordButton.Visibility = Visibility.Collapsed;
        }
    }
}
