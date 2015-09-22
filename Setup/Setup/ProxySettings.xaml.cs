using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Eulg.Setup.Shared;

namespace Eulg.Setup
{
    public partial class ProxySettings
    {
        private readonly ProxyConfig _config;
        private readonly ICollection<ValidationError> _errors; 

        private string _proxyHost;
        private int _proxyHttpPort;
        private int _proxySocksPort;
        private string _proxyUser;
        private string _proxyPass;
        private string _proxyDomain;

        public ProxySettings()
        {
            _errors = new HashSet<ValidationError>();
            _config = new ProxyConfig();
            _config.ReadFromRegistry();

            RememberCurrentConfig();
            InitializeComponent();

            ClearPasswordButton.Visibility = string.IsNullOrEmpty(_config.Password) ? Visibility.Collapsed : Visibility.Visible;
        }

        public static void ShowProxySettings()
        {
            var configControl = new ProxySettings();

            MainWindow.Instance.ShowCustomDialog(configControl, HorizontalAlignment.Right,
                Tuple.Create<string, Func<bool>>("Speichern", configControl.OnSaveProxySettings_Click),
                Tuple.Create<string, Func<bool>>("Schließen", configControl.OnLeaveProxySettings_Click));
        }

        public ProxyConfig ProxyConfig => _config;

        private bool OnSaveProxySettings_Click()
        {
            if (_errors.Any())
            {
                if (string.IsNullOrEmpty(TextBoxHttpPort.Text))
                    TextBoxHttpPort.Text = "0";
                if (string.IsNullOrEmpty(TextBoxSocksPort.Text))
                    TextBoxSocksPort.Text = "0";
                MessageBox.Show(MainWindow.Instance, "Ein oder mehrere Felder enthalten ungültige Eingaben. Bitte überprüfen Sie Ihre Einstellungen.",
                                "EULG Setup", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            try
            {
                ProxyConfig.WriteToRegistry();
                ProxyConfig.SetDefault();
                RememberCurrentConfig();
                return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(MainWindow.Instance, "Leider wurden Ihre Einstellungen nicht erfolgreich gespeichert. Systemmeldung:" +
                    Environment.NewLine + Environment.NewLine +
                    ex.GetType().Name + ": " + ex.Message, "EULG Setup", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
        }

        private bool OnLeaveProxySettings_Click()
        {
            if (_proxyHost != _config.Address
                || _proxyHttpPort != _config.HttpPort
                || _proxySocksPort != _config.Socks5Port
                || _proxyUser != _config.Username
                || _proxyPass != _config.Password
                || _proxyDomain != _config.Domain)
            {
                return MessageBox.Show(MainWindow.Instance, "Ihre geänderten Einstellungen wurden noch nicht gespeichert! Möchten Sie die Einstellungen wirklich verwerfen?",
                                       "EULG Setup", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            }

            return true;
        }

        private void RememberCurrentConfig()
        {
            _proxyHost = _config.Address;
            _proxyHttpPort = _config.HttpPort;
            _proxySocksPort = _config.Socks5Port;
            _proxyUser = _config.Username;
            _proxyPass = _config.Password;
            _proxyDomain = _config.Domain;
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(sender is PasswordBox);
            _config.Password = ((PasswordBox)sender).Password;

            ClearPasswordButton.Visibility = string.IsNullOrEmpty(_config.Password) ? Visibility.Collapsed : Visibility.Visible;
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
            _config.Password = string.Empty;
            ClearPasswordButton.Visibility = Visibility.Collapsed;
        }
    }
}
