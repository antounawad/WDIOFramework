using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

                if (args.Any(a => a.Equals("/TS", StringComparison.OrdinalIgnoreCase)) || SetupHelper.IsTerminalServerSession())
                {
                    SetupHelper.TerminalServer = true;
                }

#if !DEBUG
                if (!SetupHelper.IsAdministrator())
                {
                    MessageBox.Show("Setup bitte als Administrator ausführen!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Current.Shutdown();
                    return;
                }
#endif

                SetupHelper.UserName = args.Where(a => a.StartsWith("/A:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring(3)).FirstOrDefault();
                SetupHelper.Password = args.Where(a => a.StartsWith("/P:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring(3)).FirstOrDefault();

                SetupHelper.OfflineInstall = args.Any(a => a.Equals("/O", StringComparison.OrdinalIgnoreCase));
                SetupHelper.NoDependencies = args.Any(a => a.Equals("/NODEP", StringComparison.OrdinalIgnoreCase));
                if (SetupHelper.OfflineInstall)
                {
                    SetupHelper.OfflineZipFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eulg.zip");
                    if (!File.Exists(SetupHelper.OfflineZipFile)) throw new FileNotFoundException("Offline-Zip-Datei nicht gefunden! ", SetupHelper.OfflineZipFile);
                }

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
                    _version = ""; //TODO Version aus Client-Exe oder Uninstall-Record lesen
                    _branding = Branding.Read(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Branding.xml"));
                }
                else
                {
                    var brandingApi = new BrandingApi(config.ServiceUrl, config.Channel);
                    var brandingInfo = brandingApi.GetBranding();
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

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // 0 Setup abgebrochen
            // 1 Setup erfolgreich
            // 2 Fehler - siehe Log-Datei
            e.ApplicationExitCode = SetupHelper.ExitCode;
        }
    }
}
