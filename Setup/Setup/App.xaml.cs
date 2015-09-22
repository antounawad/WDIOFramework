using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Eulg.Setup.Shared;

namespace Eulg.Setup
{
    public partial class App
    {
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
            try
            {
                ProxyConfig.Init();
                Environment.CurrentDirectory = Path.GetTempPath();

                if (Environment.GetCommandLineArgs().Any(a => a.Equals("/TS", StringComparison.CurrentCultureIgnoreCase)) || SetupHelper.IsTerminalServerSession())
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

                var accountFlag = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("/A:", StringComparison.CurrentCultureIgnoreCase));
                if (accountFlag != null)
                {
                    SetupHelper.UserName = accountFlag.Substring(3);
                }
                var passwordFlag = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("/P:", StringComparison.CurrentCultureIgnoreCase));
                if (passwordFlag != null)
                {
                    SetupHelper.Password = passwordFlag.Substring(3);
                }

                SetupHelper.OfflineInstall = Environment.GetCommandLineArgs().Any(a => a.Equals("/O", StringComparison.CurrentCultureIgnoreCase));
                SetupHelper.NoDependencies = Environment.GetCommandLineArgs().Any(a => a.Equals("/NODEP", StringComparison.CurrentCultureIgnoreCase));
                if (SetupHelper.OfflineInstall)
                {
                    SetupHelper.OfflineZipFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eulg.zip");
                    if (!File.Exists(SetupHelper.OfflineZipFile)) throw new FileNotFoundException("Offline-Zip-Datei nicht gefunden! ", SetupHelper.OfflineZipFile);
                }

                if (!Environment.GetCommandLineArgs().Any(a => a.Equals("/C", StringComparison.CurrentCultureIgnoreCase)))
                {
                    if (SetupHelper.AlreadyRunning())
                    {
                        MessageBox.Show("Setup läuft bereits!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        Current.Shutdown();
                        return;
                    }
                }

                if (Environment.GetCommandLineArgs().Any(a => a.Equals("/U", StringComparison.CurrentCultureIgnoreCase)))
                {
                    try
                    {
                        var dst = Path.Combine(Path.GetTempPath(), "EulgSetupTemp");
                        SetupHelper.MoveUninstallerToTemp(dst);
                        var p = new Process
                        {
                            StartInfo =
                                    {
                                        FileName = Path.Combine(dst, "Setup.exe"),
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

                if (!SetupHelper.ReadConfig())
                {
                    MessageBox.Show("Fehler beim Lesen der Konfigurationsdatei!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown();
                    // ReSharper disable once RedundantJumpStatement
                    return;
                }
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
