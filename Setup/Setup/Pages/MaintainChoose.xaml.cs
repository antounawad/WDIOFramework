using System;
using System.Windows;
using System.Windows.Controls;

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
            var tmpVersion = SetupHelper.Config.Version;
            if (!string.IsNullOrWhiteSpace(SetupHelper.Config.Branding.Info.BuildTag))
            {
                tmpVersion += " (" + SetupHelper.Config.Branding.Info.BuildTag + ")";
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
