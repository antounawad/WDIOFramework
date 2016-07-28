using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Eulg.Setup.Pages
{
    public partial class InstDependencies : UserControl, ISetupPageBase
    {
        public InstDependencies()
        {
            InitializeComponent();

            PageTitle = "Erforderliche Komponenten";
            HasPrev = true;
            HasNext = true;

            IconDependencyJre.Visibility = LabelDependencyJre.Visibility = Dependencies.NeedsJre ? Visibility.Visible : Visibility.Collapsed;
        }

        public string PageTitle { get; set; }
        public Type PrevPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrev { get; set; }
        public string NextButtonText { get; set; }

        public void OnLoad()
        {
            CheckDepends();
        }
        public void OnLoadComplete()
        {
            if (SetupHelper.OfflineInstall) OnNext();
        }
        public void OnNext()
        {
            var task = new Task(() =>
            {
                try
                {
                    Dispatcher.Invoke(new Action(() => MainWindow.Instance.NavigateToPage(SetupHelper.ProgressPage, false, true)));

                    SetupHelper.RegisterContinueAfterReboot();

                    if (Dependencies.Net451 == Dependencies.EDependencyState.Missing)
                    {
                        Dependencies.InstallNet46();
                    }
                    if (Dependencies.Cpp2008 == Dependencies.EDependencyState.Missing)
                    {
                        Dependencies.InstallCpp2008();
                    }
                    if (Dependencies.Cpp2012 == Dependencies.EDependencyState.Missing)
                    {
                        Dependencies.InstallCpp2012();
                    }
                    if (Dependencies.Ie9 == Dependencies.EDependencyState.Missing)
                    {
                        Dependencies.InstallIe9();
                    }
                    if (Dependencies.Jre == Dependencies.EDependencyState.Missing)
                    {
                        Dependencies.InstallJre();
                    }

                    SetupHelper.UnregisterContinueAfterReboot();
                }
                catch (Exception exception)
                {
                    App.Setup.LogException(exception);
                    Dispatcher.Invoke(new Action(() => MainWindow.Instance.NavigateToPage(new Failed())));
                    return;
                }

                Dispatcher.Invoke(new Action(() => MainWindow.Instance.NavigateToPage(new InstLogin())));
            });
            task.Start();
        }
        public bool OnPrev()
        {
            return true;
        }
        public bool OnClose()
        {
            return true;
        }

        private void CheckDepends()
        {
            Dependencies.CheckAll();
            IconDependencyNet.Source = new BitmapImage(new Uri(@"/Setup;component/Pages/" + GetIcon(Dependencies.Net451), UriKind.Relative));
            IconDependencyCpp2008.Source = new BitmapImage(new Uri(@"/Setup;component/Pages/" + GetIcon(Dependencies.Cpp2008), UriKind.Relative));
            IconDependencyCpp2012.Source = new BitmapImage(new Uri(@"/Setup;component/Pages/" + GetIcon(Dependencies.Cpp2012), UriKind.Relative));
            IconDependencyIe9.Source = new BitmapImage(new Uri(@"/Setup;component/Pages/" + GetIcon(Dependencies.Ie9), UriKind.Relative));

            if (Dependencies.NeedsJre)
            {
                IconDependencyJre.Source = new BitmapImage(new Uri(@"/Setup;component/Pages/" + GetIcon(Dependencies.Jre), UriKind.Relative));
            }
        }
        private string GetIcon(Dependencies.EDependencyState dependencyState)
        {
            switch (dependencyState)
            {
                case Dependencies.EDependencyState.Installed:
                    return "circle_green.png";
                case Dependencies.EDependencyState.Missing:
                    return "circle_red.png";
                case Dependencies.EDependencyState.Skipped:
                    return "circle_green.png";
                case Dependencies.EDependencyState.Error:
                    return "circle_red.png";
            }
            return null;
        }
    }
}
