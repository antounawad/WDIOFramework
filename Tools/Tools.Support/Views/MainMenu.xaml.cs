using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Eulg.Shared;

namespace Eulg.Client.SupportTool.Views
{
    public partial class MainMenu : UserControl
    {
        public MainMenu()
        {
            InitializeComponent();
            ProxyConfig.Init();
            TxtProxyAddress.Text = ProxyConfig.Address;
            TxtProxyPort.Text = ProxyConfig.HttpPort.ToString();
            TxtProxyUser.Text = ProxyConfig.Username;
            TxtProxyPassword.Password = ProxyConfig.Password;
            TxtProxyDomain.Text = ProxyConfig.Domain;
            switch (ProxyConfig.ProxyType)
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
        }

        private void BtnFernwartung_OnClick(object sender, RoutedEventArgs e)
        {
            Support.RunFernwartung();
        }

        private void BtnCheckFiles(object sender, RoutedEventArgs e)
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
                }
                );
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
                if (support.Upload(log, queue, cache))
                {
                    MessageBox.Show("Upload erfolgreich!", "Upload", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Upload fehlgeschlagen!", "Upload", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            });
            task.ContinueWith(delegate { Pager.NavigateTo(this); });
            task.Start();
        }

        private void BtnProtocolViewer(object sender, RoutedEventArgs e)
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
                ProxyConfig.ProxyType = ProxyConfig.EProxyType.Manual;
                ProxyConfig.Address = TxtProxyAddress.Text.Trim();
                ushort port;
                if (ushort.TryParse(TxtProxyPort.Text, out port))
                    ProxyConfig.HttpPort = port;
                ProxyConfig.Username = TxtProxyUser.Text.Trim();
                ProxyConfig.Password = TxtProxyPassword.Password.Trim();
                ProxyConfig.Domain = TxtProxyDomain.Text.Trim();
            }
            else if (RbProxyTypeNone.IsChecked.GetValueOrDefault(false))
            {
                ProxyConfig.ProxyType = ProxyConfig.EProxyType.None;
            }
            else
            {
                ProxyConfig.ProxyType = ProxyConfig.EProxyType.Default;
            }
            ProxyConfig.WriteToRegistry();
            ProxyConfig.SetDefault();
        }

    }
}
