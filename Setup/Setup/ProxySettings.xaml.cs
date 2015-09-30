using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            public string Address { get { return Shared.ProxyConfig.Address; } set { Shared.ProxyConfig.Address = value; } }
            public ushort HttpPort { get { return Shared.ProxyConfig.HttpPort; } set { Shared.ProxyConfig.HttpPort = value; } }
            public string Username { get { return Shared.ProxyConfig.Username; } set { Shared.ProxyConfig.Username = value; } }
            public string Domain { get { return Shared.ProxyConfig.Domain; } set { Shared.ProxyConfig.Domain = value; } }
            public string Password { get { return Shared.ProxyConfig.Password; } set { Shared.ProxyConfig.Password = value; } }
            public bool ProxyTypeDefault { get { return (Shared.ProxyConfig.ProxyType == Shared.ProxyConfig.EProxyType.Default); } set { if (value) Shared.ProxyConfig.ProxyType = Shared.ProxyConfig.EProxyType.Default; } }
            public bool ProxyTypeNone { get { return (Shared.ProxyConfig.ProxyType == Shared.ProxyConfig.EProxyType.None); } set { if (value) Shared.ProxyConfig.ProxyType = Shared.ProxyConfig.EProxyType.None; } }
            public bool ProxyTypeManual { get { return (Shared.ProxyConfig.ProxyType == Shared.ProxyConfig.EProxyType.Manual); } set { if (value) Shared.ProxyConfig.ProxyType = Shared.ProxyConfig.EProxyType.Manual; } }
        }

        public ProxyConfigWrapper ProxyConfig { get; } = new ProxyConfigWrapper();


        public class RadioButtonCheckedConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
            {
                return value.Equals(parameter);
            }

            public object ConvertBack(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
            {
                return value.Equals(true) ? parameter : Binding.DoNothing;
            }
        }

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
                                "EULG Setup", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            try
            {
                Shared.ProxyConfig.WriteToRegistry();
                Shared.ProxyConfig.SetDefault();
                RememberCurrentConfig();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(MainWindow.Instance, "Leider wurden Ihre Einstellungen nicht erfolgreich gespeichert. Systemmeldung:" +
                    Environment.NewLine + Environment.NewLine +
                    ex.GetType().Name + ": " + ex.Message, "EULG Setup", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
                                       "EULG Setup", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
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
