using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using Eulg.Shared;

namespace Eulg.Setup
{
    public partial class App
    {
        private string _version;
        private string _serviceUrl;
        private Branding _branding;
        private SetupHelper _setup;

        private static App This => (App)Current;

        internal static string Version => This._version;
        internal static string ServiceUrl => This._serviceUrl;
        internal static Branding Branding => This._branding;
        internal static SetupHelper Setup => This._setup;

        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int LeftWidth;
            public int RightWidth;
            public int TopHeight;
            public int BottomHeight;
        }

        #region Dll Imports

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);
        #endregion

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            /* 
             * Command-Line Args:
             * /U - Uninstall Step 1
             * /C - Uninstall Step 2
             * /W - Called From WebSetup
             * /TS - Terminal Server
             * /O - Offline Installation using ZIP-File
             * /A:xxx - Account
             * /P:xxx - Password
             * /NODEP - Abhängigkeiten nicht prüfen (für MSI)
            */
            var args = Environment.GetCommandLineArgs();
            try
            {
                ProxyConfig.Instance.Init();
                Environment.CurrentDirectory = Path.GetTempPath();

#if !DEBUG
                if (!SetupHelper.IsAdministrator())
                {
                    MessageBox.Show("Setup bitte als Administrator ausführen!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Current.Shutdown();
                    return;
                }
#endif

                SetupHelper.CheckOfflineInstallPackage();

                if (!args.Any(a => a.Equals("/C", StringComparison.OrdinalIgnoreCase)))
                {
                    if (SetupHelper.AlreadyRunning())
                    {
                        MessageBox.Show("Setup läuft bereits!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        Current.Shutdown();
                        return;
                    }
                }

                if (args.Any(a => a.Equals("/U", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        var dst = Path.Combine(Path.GetTempPath(), "xbAVSetupTemp");
                        SetupHelper.MoveUninstallerToTemp(dst);
                        var p = new Process
                        {
                            StartInfo =
                                    {
                                FileName = Path.Combine(dst, Path.GetFileName(Assembly.GetExecutingAssembly().Location)),
                                        Arguments = "/C" + (SetupHelper.OfflineInstall ? " /O" : String.Empty)
                                    }
                        };
                        p.Start();
                    }
                    catch (Exception exception)
                    {
                        var msg = "Setup konnte nicht in den temp. Ordner kopiert werden: " + exception.GetMessagesTree();
                        MessageBox.Show(msg, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    Current.Shutdown();
                    return;
                }

                SetupConfig config;
                if (!SetupHelper.ReadConfig(out config))
                {
                    MessageBox.Show("Fehler beim Lesen der Konfigurationsdatei!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown();
                    // ReSharper disable once RedundantJumpStatement
                    return;
                }

                _serviceUrl = config.ServiceUrl;

                if(args.Any(a => a.Equals("/C", StringComparison.OrdinalIgnoreCase)))
                {
                    _branding = Branding.Read(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Branding.xml"));
                    _version = SetupHelper.ReadInstalledVersion(_branding.Registry.MachineSettingsKey);
            }
                else
            {
                    var brandingApi = new BrandingApi(config.ServiceUrl, config.Channel);
                    var brandingInfo = brandingApi.GetBranding();
                    if (brandingInfo == null)
                    {
                        // GetBranding liefert nur dann NULL wenn die URL vom UpdateService nicht ermittelt werden konnte; in dem Fall wird von der Methode selbst eine Meldung gezeigt
                Current.Shutdown();
                // ReSharper disable once RedundantJumpStatement
                return;
            }

                    if(!string.IsNullOrEmpty(brandingInfo.Message))
                    {
                        MessageBox.Show("Fehler beim Abrufen des Anwendungsprofils: " + brandingInfo.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        Current.Shutdown();
                        // ReSharper disable once RedundantJumpStatement
                        return;
                    }

                    _version = brandingInfo.Version;
                    _branding = brandingInfo.Branding;
        }

                _setup = new SetupHelper(config, _branding, _version);
            }
            catch (Exception exception)
        {
                MessageBox.Show(exception.GetMessagesTree(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }
    }
}
