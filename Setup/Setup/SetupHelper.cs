using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;
using Eulg.Setup.Pages;
using Eulg.Setup.Shared;
using Eulg.Shared;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using File = System.IO.File;

namespace Eulg.Setup
{
    public static class SetupHelper
    {
        private const string SOFTWARE_CURR_VER_REG = @"Software\Microsoft\Windows\CurrentVersion";
        private const string SOFTWARE_UNINSTALL_REG = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
        private const int KILL_PROCESS_TIMEOUT = 60;
        private const int THREAD_SLEEP_TIME = 500;
        public const string REG_KEY_SOFTWARE = "Software";
        public const string APP_DATA_DIR_DATA = "Data";
        public const string UPDATE_SERVICE_NAME = "EulgUpdate";
        private const string SETUP_EXE = "Setup.exe";
        private const string URL_WEBVERWALTUNG = @"https://verwaltung.eulg.de";
        private const string REGKEY_VERWALTUNG_DESKTOPICON_CREATED = @"VerwaltungDesktopIconCreated";

        public static string DefaultInstallPath;
        public static readonly UpdateClient UpdateClient = new UpdateClient();
        public static SetupConfig Config;
        public static Mutex AppInstanceMutex;

        public static string InstallPath { get; set; }
        public static string UserName { get; set; }
        public static string Password { get; set; }
        public static bool TerminalServer { get; set; }
        public static bool OfflineInstall { get; set; }
        public static bool NoDependencies { get; set; }
        public static string OfflineZipFile { get; set; }
        public static int ExitCode { get; set; }
        public static InstProgress ProgressPage { get; set; }

        public static bool ReadConfig()
        {
            if (Config != null)
            {
                return true;
            }
            try
            {
                var xmlSer = new XmlSerializer(typeof(SetupConfig));
                var configFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Setup.xml");
                Config = xmlSer.Deserialize(File.OpenRead(configFileName)) as SetupConfig;

                if (Config != null)
                {
                    DefaultInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), Config.Branding.FileSystem.AppDir);
                    InstallPath = DefaultInstallPath;

                    var version = new Version(Config.Version);
                    if (version.Major == 1 && version.Minor == 4)
                    {
                        if (Registry.LocalMachine.OpenSubKey(REGISTRY_GROUP_NAME)?.OpenSubKey(Config.Branding.Registry.MachineSettingsKey) == null)
                        {
                            RegKeyParent = REGISTRY_GROUP_NAME_OBSOLETE;
                        }
                    }
                }

                InitUpdateClient();

                return Config != null;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            return false;
        }

        public static bool CheckInstallation()
        {
            try
            {
                var regKey = REG_KEY_SOFTWARE + @"\" + RegKeyParent + @"\" + Config.Branding.Registry.MachineSettingsKey;
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(regKey))
                {
                    var installDir = key?.GetValue("Install_Dir") as string;
                    if (!string.IsNullOrEmpty(installDir))
                    {
                        InstallPath = installDir;
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        public static bool CheckRunningProcesses()
        {
            try
            {
                string[] killProcessNames = { Path.GetFileNameWithoutExtension(Config.Branding.FileSystem.AppBinary), Path.GetFileNameWithoutExtension(Config.Branding.FileSystem.SyncBinary) };
                var swKill = new Stopwatch();
                swKill.Start();
                var killProcesses = GetRunningProcesses(killProcessNames);
                foreach (var process in killProcesses)
                {
                    process.Kill();
                }
                while (killProcesses.Any() && swKill.ElapsedMilliseconds < (KILL_PROCESS_TIMEOUT * 1000))
                {
                    Thread.Sleep(THREAD_SLEEP_TIME);
                    killProcesses = GetRunningProcesses(killProcessNames);
                }
                return !killProcesses.Any();
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        public static void ReportProgress(string line1, string line2, int percent)
        {
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                if (percent >= 0)
                {
                    ProgressPage.ProgressBar.IsIndeterminate = false;
                    ProgressPage.ProgressBar.Value = percent;
                }
                else
                {
                    ProgressPage.ProgressBar.IsIndeterminate = true;
                }
                if (line1 != null)
                {
                    ProgressPage.LabelProgress.Content = line1;
                }
                if (line2 != null)
                {
                    ProgressPage.LabelFileCount.Content = line2;
                }
            }));
        }

        public static bool CancelRequested { get; set; }

        public const string REGISTRY_GROUP_NAME = "EULG Software GmbH";
        public const string REGISTRY_GROUP_NAME_OBSOLETE = "KS Software GmbH";
        public static string RegKeyParent { get; set; } = REGISTRY_GROUP_NAME;

        #region Install

        public static void InitUpdateClient()
        {
            UpdateClient.UpdateChannel = Config.Branding.Update.Channel;
            UpdateClient.UpdateUrl = Config.Branding.Urls.Update;
            UpdateClient.UseHttps = Config.Branding.Update.UseHttps;
        }

        public static UpdateClient.EUpdateCheckResult DownloadManifest()
        {
            UpdateClient.UserName = UserName;
            UpdateClient.Password = Password;
            var result = UpdateClient.DownloadManifest();
            if (UpdateClient?.UpdateConf != null && UpdateClient.UpdateConf.BrandingVersion > Branding.BrandingVersion)
            {
                MessageBox.Show("Dieser Web-Installer ist leider nicht mehr aktuell. Bitte verwenden Sie einen aktuellen Installer!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Error);
                return UpdateClient.EUpdateCheckResult.Error;
            }
            return result;
        }

        public static bool DownloadAppDir(bool removeAdditionalFiles = false)
        {
            UpdateClient.ApplicationPath = InstallPath;
            if (!Directory.Exists(InstallPath))
            {
                Directory.CreateDirectory(InstallPath);
            }

            UpdateClient.ProgressChanged += (sender1, args) => Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                ProgressPage.ProgressBar.IsIndeterminate = false;
                ProgressPage.ProgressBar.Value = args.ProgressPercentage;
                ProgressPage.LabelProgress.Content = args.UserState.ToString();
                ProgressPage.LabelFileCount.Content = $"{Math.Min(UpdateClient.DownloadFilesCompleted + 1, UpdateClient.DownloadFilesTotal)} / {UpdateClient.DownloadFilesTotal}";
            }));

            var ok = false;
            var result = UpdateClient.CheckForUpdates();
            switch (result)
            {
                case UpdateClient.EUpdateCheckResult.UpdatesAvailable:
                    ok = UpdateClient.DownloadUpdatesStream();
                    break;
                case UpdateClient.EUpdateCheckResult.UpToDate:
                    ok = true;
                    break;
                case UpdateClient.EUpdateCheckResult.Error:
                    break;
            }
            if (ok)
            {
                Tools.SetDirectoryAccessControl(InstallPath);
                try { Tools.SetDirectoryAccessControl(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "EULG Software GmbH")); } catch { }
                if (removeAdditionalFiles)
                {
                    // noch nicht implementiert.
                }
            }
            return ok;
        }

        public static bool PrepareRegistry()
        {
            try
            {
                PrepareHKeyCurrentUserKeys();
                PrepareHKeyLokalMachine();
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        private static void PrepareHKeyCurrentUserKeys()
        {
            // HKEY_CURRENT_USER
            using (var keySoftware = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true))
            {
                using (var keyKs = keySoftware.OpenSubKey(RegKeyParent, true) ?? keySoftware.CreateSubKey(RegKeyParent))
                {
                    using (var keyEulg = keyKs.OpenSubKey(Config.Branding.Registry.UserSettingsKey, true) ?? keyKs.CreateSubKey(Config.Branding.Registry.UserSettingsKey))
                    {
                        // If master login, don't save user values in registry
                        if (!UserName.Equals("master", StringComparison.InvariantCultureIgnoreCase))
                        {
                            HandleSyncTrayIconKey(keyEulg);

                            // Account-SubKey
                            var keyAccounts = keyEulg.OpenSubKey("Account", true) ?? keyEulg.CreateSubKey("Account");

                            var account = UpdateClient.UpdateConf.Accounts.FirstOrDefault(f => f.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase));
                            if (account == null && OfflineInstall && !string.IsNullOrEmpty(UserName))
                            {
                                account = new UpdateConfig.Account { AgencyID = string.Empty, AgencyName = string.Empty, AgencyNameShort = string.Empty, UserName = UserName };
                            }
                            if (account != null)
                            {
                                keyAccounts.SetValue(null, account.UserName, RegistryValueKind.String);

                                var keyAccount = keyAccounts.OpenSubKey(account.UserName, true) ?? keyAccounts.CreateSubKey(account.UserName);
                                if (!string.IsNullOrEmpty(account.AgencyID) && !string.IsNullOrEmpty(account.AgencyName))
                                {
                                    keyAccount.SetValue("AgencyId", account.AgencyID, RegistryValueKind.String);
                                    keyAccount.SetValue("AgencyName", account.AgencyName, RegistryValueKind.String);
                                    keyAccount.SetValue("LastLogin", $"{DateTime.Now:yyyyMMddHHmmss}", RegistryValueKind.String);
                                }
                                if (!string.IsNullOrEmpty(Password))
                                {
                                    keyAccount.SetValue("LoginPassword", EncryptPassword(Password ?? string.Empty), RegistryValueKind.Binary);
#if DEBUG
                                    keyAccount.SetValue("LoginPasswordDebug", Password ?? string.Empty, RegistryValueKind.String);
#else
                                    keyAccount.DeleteValue("LoginPasswordDebug", false);
#endif
                                }
                            }

                            // Alte zugangsdaten
                            if (!string.IsNullOrWhiteSpace(Config.Branding.Registry.LocalDbInstance) && !string.IsNullOrWhiteSpace(UserName))
                            {
                                keyEulg.SetValue(Crypt.REG_VAL_NAME_USRNAME, Crypt.EncryptUsername(UserName ?? string.Empty));
                                if (!string.IsNullOrEmpty(Password))
                                {
                                    keyEulg.SetValue(Crypt.REG_VAL_NAME_PASS, Crypt.EncryptPassword(Password));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void HandleSyncTrayIconKey(RegistryKey regKey)
        {
            const string KEY = "sync_set_tray_status";
            var valueFromReg = regKey.GetValue(KEY);
            if (valueFromReg == null)
            {
                regKey.SetValue("sync_set_tray_status", 1, RegistryValueKind.DWord);
            }
        }

        private static void PrepareHKeyLokalMachine()
        {
            // HKEY_LOCAL_MACHINE
            using (var keyLmSoftware = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true))
            {
                using (var keyLmKs = keyLmSoftware.OpenSubKey(RegKeyParent, true) ?? keyLmSoftware.CreateSubKey(RegKeyParent))
                {
                    using (var keyLmEulg = keyLmKs.OpenSubKey(Config.Branding.Registry.MachineSettingsKey, true) ?? keyLmKs.CreateSubKey(Config.Branding.Registry.MachineSettingsKey))
                    {
                        keyLmEulg.SetValue("Install_Dir", InstallPath, RegistryValueKind.String);
                        keyLmEulg.SetValue("Version", Config.Version, RegistryValueKind.String);
                        keyLmEulg.SetValue("Channel", Config.Branding.Update.Channel.ToString(), RegistryValueKind.String);
                        keyLmEulg.SetValue("TerminalServer", TerminalServer ? 1 : 0, RegistryValueKind.DWord);
                        using (var memoryStream = new MemoryStream())
                        {
                            new BinaryFormatter().Serialize(memoryStream, DateTime.Now);
                            keyLmEulg.SetValue("InstallDate", memoryStream.ToArray(), RegistryValueKind.Binary);
                        }
                    }
                }
            }
        }

        public static bool InstallUpdateService()
        {
            try
            {
                if (ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
                {
                    RemoveUpdateService();
                }
                var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "EULG Software GmbH", "UpdateService", "UpdateService.exe");
                var p = new Process
                {
                    StartInfo =
                            {
                                FileName = exePath,
                                Arguments = "install",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                };
                p.Start();
                p.WaitForExit(30 * 1000);
                if (p.HasExited)
                {
                    if (p.ExitCode == 0)
                    {
                        return true;
                    }
                    Log(UpdateClient.ELogTypeEnum.Error, "UpdateService Installation exited with return code: " + p.ExitCode);
                }
                else
                {
                    Log(UpdateClient.ELogTypeEnum.Error, "UpdateService Installation did not exit.");
                }
                return false;
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        public static bool AddShellIcons()
        {
            try
            {
                var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Config.Branding.ShellIcons.DesktopApp + ".lnk");
                DeleteIfExists(file);
                file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Config.Branding.ShellIcons.StartMenuFolder, Config.Branding.ShellIcons.StartMenuApp + ".lnk");
                DeleteIfExists(file);
                file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Config.Branding.ShellIcons.StartMenuFolder, Config.Branding.ShellIcons.StartMenuSync + ".lnk");
                DeleteIfExists(file);

                Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Config.Branding.ShellIcons.StartMenuFolder));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            try
            {
                IWshShell wsh = new WshShellClass();
                var shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                                                                          Config.Branding.ShellIcons.DesktopApp + ".lnk"));
                shtct.WindowStyle = 1; //for default window, 3 for maximize, 7 for minimize
                shtct.TargetPath = Path.Combine(InstallPath, Config.Branding.FileSystem.AppBinary);
                shtct.IconLocation = Path.Combine(InstallPath, Config.Branding.FileSystem.AppBinary) + ", 0";
                shtct.Save();

                var startMenuFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), Config.Branding.ShellIcons.StartMenuFolder);

                if (!String.IsNullOrWhiteSpace(Config.Branding.ShellIcons.StartMenuFolder) &&
                    !Directory.Exists(startMenuFolderPath))
                {
                    Directory.CreateDirectory(startMenuFolderPath);
                }

                if (!String.IsNullOrWhiteSpace(Config.Branding.ShellIcons.StartMenuApp))
                {
                    shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                                                          Config.Branding.ShellIcons.StartMenuFolder,
                                                                          Config.Branding.ShellIcons.StartMenuApp + ".lnk"));
                    shtct.WindowStyle = 1;
                    shtct.TargetPath = Path.Combine(InstallPath, Config.Branding.FileSystem.AppBinary);
                    shtct.IconLocation = Path.Combine(InstallPath, Config.Branding.FileSystem.AppBinary) + ", 0";
                    shtct.Save();
                }

                if (!String.IsNullOrWhiteSpace(Config.Branding.ShellIcons.StartMenuSync))
                {
                    shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                                                          Config.Branding.ShellIcons.StartMenuFolder,
                                                                          Config.Branding.ShellIcons.StartMenuSync + ".lnk"));
                    shtct.WindowStyle = 1;
                    shtct.TargetPath = Path.Combine(InstallPath, Config.Branding.FileSystem.SyncBinary);
                    shtct.IconLocation = Path.Combine(InstallPath, Config.Branding.FileSystem.SyncBinary) + ", 0";
                    shtct.Save();
                }

                if (!String.IsNullOrWhiteSpace(Config.Branding.ShellIcons.StartMenuSupportTool))
                {
                    shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                                                          Config.Branding.ShellIcons.StartMenuFolder,
                                                                          Config.Branding.ShellIcons.StartMenuSupportTool + ".lnk"));
                    shtct.WindowStyle = 1;
                    shtct.TargetPath = Path.Combine(InstallPath, "Support", "EulgSupport.exe");
                    shtct.IconLocation = shtct.TargetPath + ", 0";
                    shtct.Save();
                }
                if (!String.IsNullOrWhiteSpace(Config.Branding.ShellIcons.StartMenuFernwartung))
                {
                    shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                                                          Config.Branding.ShellIcons.StartMenuFolder,
                                                                          Config.Branding.ShellIcons.StartMenuFernwartung + ".lnk"));
                    shtct.WindowStyle = 1;
                    shtct.TargetPath = Path.Combine(InstallPath, "Support", "EulgFernwartung.exe");
                    shtct.IconLocation = shtct.TargetPath + ", 0";
                    shtct.Save();
                }
                // Link zur Webverwaltung
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE + @"\" + RegKeyParent + @"\" + Config.Branding.Registry.UserSettingsKey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
                {
                    if (key != null && key.GetValue(REGKEY_VERWALTUNG_DESKTOPICON_CREATED, null) == null)
                    {
                        var urlWebverwLnkFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Webverwaltung" + ".lnk");
                        if (!File.Exists(urlWebverwLnkFile))
                        {
                            shtct = (IWshShortcut)wsh.CreateShortcut(urlWebverwLnkFile);
                            shtct.WindowStyle = 3;
                            shtct.TargetPath = URL_WEBVERWALTUNG;
                            shtct.IconLocation = Path.Combine(InstallPath, Config.Branding.FileSystem.SyncBinary) + ", 0";
                            shtct.Save();
                        }
                        key.SetValue(REGKEY_VERWALTUNG_DESKTOPICON_CREATED, $"{DateTime.Now:u}", RegistryValueKind.String);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        private static void DeleteIfExists(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        public static bool InstallUninstaller()
        {
            try
            {
                var src = AppDomain.CurrentDomain.BaseDirectory;
                var dst = Path.Combine(InstallPath, "Setup");

                if (!Directory.Exists(dst))
                {
                    Directory.CreateDirectory(dst);
                }
                var filesToCopyOver = new[] { "Setup.exe", "Setup.xml", "Interop.IWshRuntimeLibrary.dll" };
                foreach (var fileToCopyOver in filesToCopyOver)
                {
                    if (File.Exists(Path.Combine(src, fileToCopyOver)))
                    {
                        File.Copy(Path.Combine(src, fileToCopyOver), Path.Combine(dst, fileToCopyOver), true);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        public static bool RegisterUninstaller()
        {
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                                        .OpenSubKey(SOFTWARE_UNINSTALL_REG, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
            {
                var subKey = key.OpenSubKey(Config.Branding.Registry.UninstallKey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl)
                             ?? key.CreateSubKey(Config.Branding.Registry.UninstallKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (subKey == null)
                {
                    throw new Exception("Kann Uninstall nicht schreiben!");
                }

                subKey.SetValue("DisplayName", Config.Branding.Registry.UninstallName, RegistryValueKind.String);
                subKey.SetValue("DisplayIcon", Path.Combine(InstallPath, "Setup", SETUP_EXE), RegistryValueKind.String);
                subKey.SetValue("Publisher", "EULG Software GmbH", RegistryValueKind.String);
                subKey.SetValue("URLInfoAbout", "www.eulg.de", RegistryValueKind.String);
                subKey.SetValue("UninstallString", Path.Combine(InstallPath, "Setup", SETUP_EXE) + " /U", RegistryValueKind.String);
                subKey.SetValue("InstallDate", $"{DateTime.Now:YYYYMMDD}", RegistryValueKind.String);
                subKey.SetValue("InstallLocation", InstallPath, RegistryValueKind.String);
                var kbNeeded = OfflineInstall ? new FileInfo(OfflineZipFile).Length / 1024 : UpdateClient.UpdateConf.UpdateFiles.Sum(s => s.FileSize) / 1024;
                subKey.SetValue("EstimatedSize", kbNeeded, RegistryValueKind.DWord);
                subKey.SetValue("Language", 1033, RegistryValueKind.DWord);
                subKey.SetValue("NoModify", 0, RegistryValueKind.DWord);
                subKey.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                subKey.SetValue("ModifyPath", Path.Combine(InstallPath, "Support", "EulgSupport.exe"), RegistryValueKind.String);

                var tmpVersion = Config.Version;
                if (!String.IsNullOrWhiteSpace(Config.Branding.Info.BuildTag))
                {
                    tmpVersion += " (" + Config.Branding.Info.BuildTag + ")";
                }
                if (TerminalServer)
                {
                    tmpVersion += " TerminalServer";
                }
                subKey.SetValue("DisplayVersion", tmpVersion, RegistryValueKind.String);
                Version version;
                if (Version.TryParse(Config.Version, out version))
                {
                    subKey.SetValue("Version", version.Build, RegistryValueKind.DWord);
                    subKey.SetValue("VersionMajor", version.Major, RegistryValueKind.DWord);
                    subKey.SetValue("VersionMinor", version.Minor, RegistryValueKind.DWord);
                }
            }
            return true;
        }

        public static void RegisterContinueAfterReboot()
        {
            try
            {
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
                                            .OpenSubKey(SOFTWARE_CURR_VER_REG, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
                {
                    var keyRunOnce = key.OpenSubKey("RunOnce", true) ?? key.CreateSubKey("RunOnce");
                    keyRunOnce.SetValue("EulgSetup", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SETUP_EXE) + " /D");
                }
            }
            catch
            {
            }
        }

        public static void UnregisterContinueAfterReboot()
        {
            try
            {
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default)
                                            .OpenSubKey(SOFTWARE_CURR_VER_REG, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
                {
                    var keyRunOnce = key.OpenSubKey("RunOnce", true) ?? key.CreateSubKey("RunOnce");
                    keyRunOnce.DeleteValue("EulgSetup", false);
                }
            }
            catch
            {
            }
        }

        public static bool WriteBranding()
        {
            try
            {
                Config.Branding.Write(Path.Combine(InstallPath, "Branding.xml"));
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        public static bool InstallNgen()
        {
            try
            {
                var ngenBinary = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
                RunProcess(ngenBinary, "queue pause", InstallPath);
                if (!string.IsNullOrWhiteSpace(Config.Branding.FileSystem.AppBinary))
                {
                    RunProcess(ngenBinary, $"install \"{Path.Combine(InstallPath, Config.Branding.FileSystem.AppBinary)}\" /queue:1", InstallPath);
                }
                if (!string.IsNullOrWhiteSpace(Config.Branding.FileSystem.SyncBinary))
                {
                    RunProcess(ngenBinary, $"install \"{Path.Combine(InstallPath, Config.Branding.FileSystem.SyncBinary)}\" /queue:1", InstallPath);
                }
                RunProcess(ngenBinary, "queue continue", InstallPath);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            return true;
        }

        public static void ExtractAppDir()
        {
            UpdateClient.ApplicationPath = InstallPath;
            if (!Directory.Exists(InstallPath))
            {
                Directory.CreateDirectory(InstallPath);
            }
            dynamic shellApplication = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
            dynamic compressedFolderContents = shellApplication.NameSpace(OfflineZipFile).Items;
            dynamic destinationFolder = shellApplication.NameSpace(InstallPath);
            destinationFolder.CopyHere(compressedFolderContents, 16);

            var srcFile = Path.Combine(InstallPath, "UpdateService.exe");
            var dstPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "EULG Software GmbH", "UpdateService");
            var dstFile = Path.Combine(dstPath, "UpdateService.exe");
            if (!Directory.Exists(dstPath))
            {
                Directory.CreateDirectory(dstPath);
            }
            File.Copy(srcFile, dstFile, true);
            File.Delete(srcFile);

            /*
               4 – Do not display a progress dialog box.
               8 – Give the file being operated on a new name in a move, copy, or rename operation if a file with the target name already exists.
              16 – Respond with “Yes to All” for any dialog box that is displayed.
              64 – Preserve undo information, if possible.
             128 – Perform the operation on files only if a wildcard file name (*.*) is specified.
             256 – Display a progress dialog box but do not show the file names.
             512 – Do not confirm the creation of a new directory if the operation requires one to be created.
            1024 – Do not display a user interface if an error occurs.
            2048 – Version 4.71. Do not copy the security attributes of the file.
            4096 – Only operate in the local directory. Do not operate recursively into subdirectories.
            8192 – Version 5.0. Do not copy connected files as a group. Only copy the specified files.
            */
        }

        #endregion

        #region Remove

        public static bool RemoveUpdateService()
        {
            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
            {
                return true;
            }
            var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "EULG Software GmbH", "UpdateService", "UpdateService.exe");
            if (!File.Exists(exePath))
            {
                return true;
            }
            try
            {
                StopService(true);
                var p = new Process
                {
                    StartInfo =
                            {
                                FileName = exePath,
                                Arguments = "uninstall",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                };
                p.Start();
                p.WaitForExit(30 * 1000);
                if (p.HasExited)
                {
                    if (p.ExitCode == 0)
                    {
                        return true;
                    }
                    Log(UpdateClient.ELogTypeEnum.Error, "UpdateService De-Installation exited with return code: " + p.ExitCode);
                }
                else
                {
                    Log(UpdateClient.ELogTypeEnum.Error, "UpdateService De-Installation did not exit.");
                }
                return false;
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        public static bool RemoveShellIcons()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), Config.Branding.ShellIcons.DesktopApp + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                Config.Branding.ShellIcons.StartMenuFolder,
                                Config.Branding.ShellIcons.StartMenuApp + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                Config.Branding.ShellIcons.StartMenuFolder,
                                Config.Branding.ShellIcons.StartMenuSync + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Config.Branding.ShellIcons.DesktopApp + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                                Config.Branding.ShellIcons.StartMenuFolder,
                                Config.Branding.ShellIcons.StartMenuApp + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                                Config.Branding.ShellIcons.StartMenuFolder,
                                Config.Branding.ShellIcons.StartMenuSync + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                                Config.Branding.ShellIcons.StartMenuFolder,
                                Config.Branding.ShellIcons.StartMenuSupportTool + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                                Config.Branding.ShellIcons.StartMenuFolder,
                                Config.Branding.ShellIcons.StartMenuFernwartung + ".lnk");
            DeleteIfExists(file);

            try
            {
                Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), Config.Branding.ShellIcons.StartMenuFolder));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            try
            {
                Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Config.Branding.ShellIcons.StartMenuFolder));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            return true;
        }

        public static bool ClearAppData()
        {
            return true;
        }

        public static bool ClearAppDir()
        {
            DelTree(InstallPath);
            return true;
        }

        public static bool ClearReg()
        {
            try
            {
                //HKEY_LOCAL_MACHINE
                using (var keyLmSoftware = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true))
                {
                    if (keyLmSoftware != null)
                    {
                        using (var keyLmKs = keyLmSoftware.OpenSubKey(REGISTRY_GROUP_NAME, true))
                        {
                            keyLmKs?.DeleteSubKeyTree(Config.Branding.Registry.MachineSettingsKey, false);
                        }
                        using (var keyLmKs = keyLmSoftware.OpenSubKey(REGISTRY_GROUP_NAME_OBSOLETE, true))
                        {
                            keyLmKs?.DeleteSubKeyTree(Config.Branding.Registry.MachineSettingsKey, false);
                        }
                    }
                }

                // HKEY_CURRENT_USER

                using (var keySoftware = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true))
                {
                    if (keySoftware != null)
                    {
                        using (var keyKs = keySoftware.OpenSubKey(REGISTRY_GROUP_NAME, true))
                        {
                            keyKs?.DeleteSubKeyTree(Config.Branding.Registry.UserSettingsKey, false);
                        }
                        using (var keyKs = keySoftware.OpenSubKey(REGISTRY_GROUP_NAME_OBSOLETE, true))
                        {
                            keyKs?.DeleteSubKeyTree(Config.Branding.Registry.UserSettingsKey, false);
                        }
                    }
                }

                using (var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(SOFTWARE_CURR_VER_REG, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
                {
                    if (key != null)
                    {
                        using (var keyRunOnce = key.OpenSubKey("Run", true) ?? key.CreateSubKey("Run"))
                        {
                            keyRunOnce?.DeleteValue(Config.Branding.Registry.UserSettingsKey + "-Sync", false);
                        }
                    }
                }

                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        public static bool UnregisterUninstall()
        {
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                                        .OpenSubKey(SOFTWARE_UNINSTALL_REG, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
            {
                if (key?.OpenSubKey(Config.Branding.Registry.UninstallKey) != null)
                {
                    key.DeleteSubKeyTree(Config.Branding.Registry.UninstallKey);
                }
            }
            return true;
        }

        public static void MoveUninstallerToTemp(string tmpDir)
        {
            var src = AppDomain.CurrentDomain.BaseDirectory;
            DelTree(tmpDir);
            if (!Directory.Exists(tmpDir))
            {
                Directory.CreateDirectory(tmpDir);
            }
            foreach (var f in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
            {
                var df = Path.Combine(tmpDir, f.Substring(src.Length));
                var dp = Path.GetDirectoryName(df) ?? "";
                if (!Directory.Exists(dp))
                {
                    Directory.CreateDirectory(dp);
                }
                File.Copy(f, Path.Combine(tmpDir, f.Substring(src.Length)), true);
            }
        }

        public static bool UninstallNgen()
        {
            try
            {
                var ngenBinary = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
                if (!string.IsNullOrWhiteSpace(Config.Branding.FileSystem.AppBinary))
                {
                    RunProcess(ngenBinary, $"uninstall \"{Path.Combine(InstallPath, Config.Branding.FileSystem.AppBinary)}\"", InstallPath);
                }
                if (!string.IsNullOrWhiteSpace(Config.Branding.FileSystem.SyncBinary))
                {
                    RunProcess(ngenBinary, $"uninstall \"{Path.Combine(InstallPath, Config.Branding.FileSystem.SyncBinary)}\"", InstallPath);
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            return true;
        }

        #endregion

        #region Tools

        private static Process[] GetRunningProcesses(IEnumerable<string> processNames)
        {
            var list = new List<Process>();
            foreach (var processName in processNames)
            {
                var runningProcesses = Process.GetProcessesByName(processName);
                list.AddRange(runningProcesses);
            }
            return list.ToArray();
        }

        public static void Log(UpdateClient.ELogTypeEnum logType, string message)
        {
            UpdateClient.Log(logType, message);
        }

        public static void LogException(Exception exception)
        {
            Log(UpdateClient.ELogTypeEnum.Error, exception.GetMessagesTree());
        }

        public static void StopService(bool wait)
        {
            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
            {
                return;
            }
            using (var svc = new ServiceController(UPDATE_SERVICE_NAME))
            {
                if (svc.Status != ServiceControllerStatus.Stopped)
                {
                    svc.Stop();
                }
                if (wait)
                {
                    svc.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
        }

        public static void DelTree(string path)
        {
            // ReSharper disable EmptyGeneralCatchClause
            try
            {
                foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            try
            {
                foreach (var directory in Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly))
                {
                    DelTree(directory);
                    try
                    {
                        Directory.Delete(directory);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            try
            {
                Directory.Delete(path);
            }
            catch
            {
            }
            // ReSharper enable EmptyGeneralCatchClause
        }

        public static bool TryDelFile(string fileName, int timeout)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (File.Exists(fileName) && stopwatch.ElapsedMilliseconds < timeout)
            {
                try
                {
                    File.Delete(fileName);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        public static bool AlreadyRunning()
        {
            AppInstanceMutex = MutexManager.Instance.Acquire("WebInstaller", TimeSpan.Zero);
            return AppInstanceMutex == null;
        }

        public static bool IsAdministrator()
        {
            var currentIdentity = WindowsIdentity.GetCurrent();
            if (currentIdentity == null)
            {
                return false;
            }
            return new WindowsPrincipal(currentIdentity).IsInRole(WindowsBuiltInRole.Administrator);
        }

        //public static void CleanUp()
        //{
        //    DelTree(Path.Combine(Path.GetTempPath(), "EulgWebInstaller"));
        //    DelTree(Path.Combine(Path.GetTempPath(), "EulgSetupTemp"));
        //}
        private static byte[] EncryptPassword(string str)
        {
            return ProtectedData.Protect(Encoding.UTF8.GetBytes(str), null, DataProtectionScope.CurrentUser);
        }

        private static int RunProcess(string fileName, string arguments, string workingDirectory)
        {
            var process = new Process
            {
                StartInfo =
                              {
                                  FileName = fileName,
                                  Arguments = arguments,
                                  WorkingDirectory = workingDirectory,
                                  CreateNoWindow = true,
                                  UseShellExecute = false,
                                  WindowStyle = ProcessWindowStyle.Hidden,
                                  RedirectStandardError = true,
                                  RedirectStandardOutput = true
                              }
            };
            process.Start();
            process.WaitForExit();
            var err = process.StandardError.ReadToEnd();

            if (!String.IsNullOrWhiteSpace(err))
            {
                Log(UpdateClient.ELogTypeEnum.Warning, err);
            }

            return process.ExitCode;
        }

        public static bool IsTerminalServerSession()
        {
            return System.Windows.Forms.SystemInformation.TerminalServerSession;
        }

        #endregion
    }
}
