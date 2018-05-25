using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Eulg.Shared;
using Microsoft.Win32;

namespace Eulg.Client.SupportTool.Views
{
    public partial class MainMenu : UserControl
    {
        private readonly ProxyConfig _proxyConfig;
        private const string REG_KEY_SOFTWARE = "Software";
        private const string REG_KEY_PARENT = "xbAV Beratungssoftware GmbH";

        public MainMenu()
        {
            InitializeComponent();

            _proxyConfig = new ProxyConfig();

            _proxyConfig.Init();
            TxtProxyAddress.Text = _proxyConfig.Address;
            TxtProxyPort.Text = _proxyConfig.HttpPort.ToString();
            TxtProxyUser.Text = _proxyConfig.Username;
            TxtProxyPassword.Password = _proxyConfig.Password;
            TxtProxyDomain.Text = _proxyConfig.Domain;
            switch (_proxyConfig.ProxyType)
            {
                case ProxyConfig.EProxyType.Default:
                    RbProxyTypeDefault.IsChecked = true;
                    break;
                case ProxyConfig.EProxyType.None:
                    RbProxyTypeNone.IsChecked = true;
                    break;
                case ProxyConfig.EProxyType.Manual:
                    RbProxyTypeManual.IsChecked = true;
                    break;
            }

            var keyLmParent = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, false)?.OpenSubKey(REG_KEY_PARENT, false);
            if (keyLmParent != null && (keyLmParent.GetValue("LimitSupportTool", null) as int? ?? 0) > 0)
            {
                BtnCheckFiles.IsEnabled = false;
                BtnCheckService.IsEnabled = false;
                BtnFernwartung.IsEnabled = false;
            }
        }

        private void BtnFernwartung_OnClick(object sender, RoutedEventArgs e)
        {
            Support.RunFernwartung();
        }

        private void BtnCheckFiles_OnClick(object sender, RoutedEventArgs e)
        {
#if !DEBUG
            if (Support.ExitClient())
#endif
            {
                var progressView = new Progress();
                Pager.NavigateTo(progressView);
                var support = new Support();
                support.ProgressChanged += (o, args) => Dispatcher.Invoke(() =>
                {
                    var a = args.UserState as string;
                    var header = string.Empty;
                    var message = string.Empty;
                    if (a != null)
                    {
                        if (a.StartsWith("*", StringComparison.CurrentCultureIgnoreCase))
                            header = a.Substring(1);
                        else
                            message = a;
                    }
                    progressView.UpdateProgress(args.ProgressPercentage, message, header);
                });
                var task = new Task(support.DoUpdateCheck);
                task.ContinueWith(delegate { Pager.NavigateTo(this); });
                task.Start();
            }
        }

        private void BtnUpload_OnClick(object sender, RoutedEventArgs e)
        {
            var log = ChkUploadProtokoll.IsChecked.GetValueOrDefault(false);
            var queue = ChkUploadQueue.IsChecked.GetValueOrDefault(false);
            var cache = ChkUploadCache.IsChecked.GetValueOrDefault(false);

            if (!log && !queue && !cache)
            {
                MessageBox.Show("Bitte mindestens eine Komponente zum Upload auswählen!", "Upload", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var progressView = new Progress();
            Pager.NavigateTo(progressView);
            var support = new Support();
            support.ProgressChanged += (o, args) => Dispatcher.Invoke(() =>
            {
                var a = args.UserState as string;
                var header = string.Empty;
                var message = string.Empty;
                if (a != null)
                {
                    if (a.StartsWith("*", StringComparison.CurrentCultureIgnoreCase))
                        header = a.Substring(1);
                    else
                        message = a;
                }
                progressView.UpdateProgress(args.ProgressPercentage, message, header);
            }
            );
            var task = new Task(() =>
            {
                var result = support.Upload(log, queue, cache);
                if (result == true)
                {
                    MessageBox.Show("Upload erfolgreich!", "Upload", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (result == false)
                {
                    MessageBox.Show("Upload fehlgeschlagen!", "Upload", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            });
            task.ContinueWith(delegate { Pager.NavigateTo(this); });
            task.Start();
        }

        private void BtnProtocolViewer_OnClick(object sender, RoutedEventArgs e)
        {
            var support = new Support();
            var logViewer = new LogViewer();
            logViewer.SetTextClient(support.GetLogEntries(Support.ELogType.Client));
            logViewer.SetTextSync(support.GetLogEntries(Support.ELogType.Sync));
            logViewer.Show();
        }

        private void OnSaveProxySettings_Click(object sender, RoutedEventArgs e)
        {
            SaveProxy();
            MessageBox.Show("Proxy-Einstellungen gespeichert.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCheckService_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Support.CheckUpdateService())
            {
                if (MessageBox.Show("Update Dienst konnte nicht gestartet werden. Dienst wird neu installiert.", "Update-Dienst", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    if (!Support.FixUpdateService())
                    {
                        MessageBox.Show("Update-Dienst konnte nicht aktiviert werden. Bitte führen Sie Setup erneut aus.", "Update-Dienst", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Update-Dienst erfolgreich installiert.", "Update-Dienst", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Update-Dienst funktioniert!", "Update-Dienst", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Contains(TabItemProxy))
            {
                SaveProxy();
            }
        }

        private void SaveProxy()
        {
            if (RbProxyTypeManual.IsChecked.GetValueOrDefault(false))
            {
                _proxyConfig.ProxyType = ProxyConfig.EProxyType.Manual;
                _proxyConfig.Address = TxtProxyAddress.Text.Trim();
                ushort port;
                if (ushort.TryParse(TxtProxyPort.Text, out port))
                    _proxyConfig.HttpPort = port;
                _proxyConfig.Username = TxtProxyUser.Text.Trim();
                _proxyConfig.Password = TxtProxyPassword.Password.Trim();
                _proxyConfig.Domain = TxtProxyDomain.Text.Trim();
            }
            else if (RbProxyTypeNone.IsChecked.GetValueOrDefault(false))
            {
                _proxyConfig.ProxyType = ProxyConfig.EProxyType.None;
            }
            else
            {
                _proxyConfig.ProxyType = ProxyConfig.EProxyType.Default;
            }
            _proxyConfig.WriteToRegistry();
            _proxyConfig.SetDefault();
        }

    }
}
