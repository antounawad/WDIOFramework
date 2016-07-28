using System;
using System.Windows;
using System.Windows.Controls;
using Eulg.Shared;

namespace Eulg.Setup.Pages
{
    public partial class MaintainChoose : UserControl, ISetupPageBase
    {
        public MaintainChoose()
        {
            InitializeComponent();

            PageTitle = "Wartungsmodus";
            HasPrev = false;
            HasNext = false;
        }

        public string PageTitle { get; set; }
        public Type PrevPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public string NextButtonText { get; set; }

        public void OnLoad()
        {
            var tmpVersion = App.Setup.Version;
            if (App.Setup.Config.Channel != Branding.EUpdateChannel.Release)
            {
                tmpVersion += " (" + App.Setup.Config.Channel + ")";
            }
            if (SetupHelper.TerminalServer)
            {
                tmpVersion += " TerminalServer";
            }
            LblVersion.Content = tmpVersion;
        }
        public void OnLoadComplete()
        {
            if (SetupHelper.OfflineInstall) OnNext();
        }
        public void OnNext() { }
        public bool OnPrev()
        {
            return true;
        }
        public bool OnClose()
        {
            return true;
        }

        private void BtnMaintainRepair_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateToPage(new InstDependencies());
        }
        private void BtnMaintainRemove_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateToPage(new MaintainUninstall());
        }

    }
}
