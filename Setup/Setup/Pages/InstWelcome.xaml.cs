﻿using System;
using System.Windows;
using System.Windows.Controls;
using Eulg.Shared;

namespace Eulg.Setup.Pages
{
    public partial class InstWelcome : UserControl, ISetupPageBase
    {
        public InstWelcome()
        {
            InitializeComponent();

            PageTitle = "Willkommen beim Installations-Assistenten";
            HasPrev = false;
            HasNext = true;
        }

        public string PageTitle { get; set; }
        public Type PrevPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public string NextButtonText { get; set; }

        public void OnLoad()
        {
            var setup = App.Setup;
            var tmpVersion = setup.Version;
            if (setup.Config.Channel != Branding.EUpdateChannel.Release)
            {
                tmpVersion += " (" + setup.Config.Channel + ")";
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
        public void OnNext()
        {
            if (!UpdateClient.CheckConnectivity())
            {
                MessageBox.Show("Keine Internetverbindung!");
                return;
            }
            if (SetupHelper.NoDependencies)
                MainWindow.Instance.NavigateToPage(new InstLogin());
            else
                MainWindow.Instance.NavigateToPage(new InstDependencies());
        }
        public bool OnPrev()
        {
            return true;
        }
        public bool OnClose()
        {
            return true;
        }

        private void EditProxySettings(object sender, RoutedEventArgs e)
        {
            ProxySettings.ShowProxySettings();
        }
    }
}
