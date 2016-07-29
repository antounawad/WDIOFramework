using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
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
    internal class SetupHelper
    {
        private const string SOFTWARE_CURR_VER_REG = @"Software\Microsoft\Windows\CurrentVersion";
        private const string SOFTWARE_UNINSTALL_REG = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
        private const int KILL_PROCESS_TIMEOUT = 60;
        private const int THREAD_SLEEP_TIME = 500;

        public const string REG_KEY_SOFTWARE = "Software";
        public const string APP_DATA_DIR_DATA = "Data";
        public const string UPDATE_SERVICE_NAME = "EulgUpdate"; //FIXME Erwähnt EULG, allerdings ist es wahrscheinlich nicht clever, hier etwas zu ändern...

        public static Mutex AppInstanceMutex;

        private static readonly Lazy<string[]> _cmdArgs = new Lazy<string[]>(Environment.GetCommandLineArgs, LazyThreadSafetyMode.None);
        public static string[] CommandLineArgs => _cmdArgs.Value;

        public static string DefaultUsername => CommandLineArgs.Where(a => a.StartsWith("/A:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring(3)).FirstOrDefault();
        public static string DefaultPassword => CommandLineArgs.Where(a => a.StartsWith("/P:", StringComparison.OrdinalIgnoreCase)).Select(a => a.Substring(3)).FirstOrDefault();
        public static bool TerminalServer => CommandLineArgs.Any(a => a.Equals("/TS", StringComparison.OrdinalIgnoreCase)) || System.Windows.Forms.SystemInformation.TerminalServerSession;
        public static bool OfflineInstall => CommandLineArgs.Any(a => a.Equals("/O", StringComparison.OrdinalIgnoreCase));
        public static bool NoDependencies => CommandLineArgs.Any(a => a.Equals("/NODEP", StringComparison.OrdinalIgnoreCase));
        public static string OfflineZipFile => !OfflineInstall ? null : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eulg.zip");

        private static IDictionary<EApiResource, Uri> _apiManifest;

        public string DefaultInstallPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), Branding.FileSystem.AppDir);
        public string InstallPath { get; set; }

        [Obsolete("wtf")]
        public static InstProgress ProgressPage { get; set; }

        public SetupConfig Config { get; }

        public Branding Branding { get; }

        public string Version { get; }

        public UpdateClient UpdateClient { get; }

        public SetupHelper(SetupConfig config, Branding branding, string version)
        {
            Config = config;
            Branding = branding;
            Version = version;
            UpdateClient = new UpdateClient(config.ApiManifestUri, config.Channel);

            InstallPath = DefaultInstallPath;

            var v = new Version(version);
            if(v.Major == 1 && v.Minor == 4)
            {
                if(Registry.LocalMachine.OpenSubKey(REGISTRY_GROUP_NAME)?.OpenSubKey(branding.Registry.MachineSettingsKey) == null)
                {
                    RegKeyParent = REGISTRY_GROUP_NAME_KS;
                }
            }
            else if (v.Major < 2)
            {
                if(Registry.LocalMachine.OpenSubKey(REGISTRY_GROUP_NAME)?.OpenSubKey(branding.Registry.MachineSettingsKey) == null)
                {
                    RegKeyParent = REGISTRY_GROUP_NAME_EULG;
                }
            }
        }

        public static void CheckOfflineInstallPackage()
        {
            if (!OfflineInstall) return;

            var file = OfflineZipFile;
            if(!File.Exists(file)) throw new FileNotFoundException("Offline-Zip-Datei nicht gefunden! ", file);
        }

        public static bool ReadConfig(out SetupConfig config)
        {
            config = null;

            try
            {
                var xmlSer = new XmlSerializer(typeof(SetupConfig));
                var configFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Setup.xml");
                config = xmlSer.Deserialize(File.OpenRead(configFileName)) as SetupConfig;

                return config != null;
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            return false;
        }

        public bool CheckInstallation()
        {
            try
            {
                var regKey = REG_KEY_SOFTWARE + @"\" + RegKeyParent + @"\" + Branding.Registry.MachineSettingsKey;
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

        public bool CheckRunningProcesses()
        {
            try
            {
                string[] killProcessNames = { Path.GetFileNameWithoutExtension(Branding.FileSystem.AppBinary), Path.GetFileNameWithoutExtension(Branding.FileSystem.SyncBinary) };
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

        public const string REGISTRY_GROUP_NAME = "xbAV Beratungssoftware GmbH";
        public const string REGISTRY_GROUP_NAME_EULG = "EULG Software GmbH";
        public const string REGISTRY_GROUP_NAME_KS = "KS Software GmbH";
        public static string RegKeyParent { get; set; } = REGISTRY_GROUP_NAME;

        #region Install

        public UpdateClient.EUpdateCheckResult DownloadManifest(string username, string password)
        {
            UpdateClient.UserName = username;
            UpdateClient.Password = password;
            var result = UpdateClient.DownloadManifest();
            if (UpdateClient?.UpdateConf != null && UpdateClient.UpdateConf.BrandingVersion > Branding.BrandingVersion)
            {
                MessageBox.Show("Dieser Web-Installer ist leider nicht mehr aktuell. Bitte verwenden Sie einen aktuellen Installer!", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Error);
                return UpdateClient.EUpdateCheckResult.Error;
            }
            return result;
        }

        public bool DownloadAppDir(bool removeAdditionalFiles = false)
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
                try { Tools.SetDirectoryAccessControl(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "xbAV Beratungssoftware GmbH")); } catch { }
                if (removeAdditionalFiles)
                {
                    // noch nicht implementiert.
                }
            }
            return ok;
        }

        public bool PrepareRegistry(Profile profile, string username, string password)
        {
            try
            {
                PrepareHKeyCurrentUserKeys(profile, username, password);
                PrepareHKeyLokalMachine(profile);
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        private void PrepareHKeyCurrentUserKeys(Profile profile, string username, string password)
        {
            // HKEY_CURRENT_USER
            using (var keySoftware = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true))
            {
                using (var keyKs = keySoftware.OpenSubKey(RegKeyParent, true) ?? keySoftware.CreateSubKey(RegKeyParent))
                {
                    using (var keyEulg = keyKs.OpenSubKey(Branding.Registry.UserSettingsKey, true) ?? keyKs.CreateSubKey(Branding.Registry.UserSettingsKey))
                    {
                        keyEulg.SetValue("CurrentTheme", profile.ProfileId, RegistryValueKind.DWord);

                        // If master login, don't save user values in registry
                        if (!username.Equals("master", StringComparison.InvariantCultureIgnoreCase))
                        {
                            HandleSyncTrayIconKey(keyEulg);

                            // Account-SubKey
                            var keyAccounts = keyEulg.OpenSubKey("Account", true) ?? keyEulg.CreateSubKey("Account");

                            var account = UpdateClient.UpdateConf.Accounts.FirstOrDefault(f => f.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase));
                            if (account == null && OfflineInstall && !string.IsNullOrEmpty(username))
                            {
                                account = new UpdateConfig.Account { AgencyID = string.Empty, AgencyName = string.Empty, AgencyNameShort = string.Empty, UserName = username };
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
                                if (!string.IsNullOrEmpty(password))
                                {
                                    keyAccount.SetValue("LoginPassword", EncryptPassword(password ?? string.Empty), RegistryValueKind.Binary);
#if DEBUG
                                    keyAccount.SetValue("LoginPasswordDebug", password ?? string.Empty, RegistryValueKind.String);
#else
                                    keyAccount.DeleteValue("LoginPasswordDebug", false);
#endif
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

        private void PrepareHKeyLokalMachine(Profile profile)
        {
            // HKEY_LOCAL_MACHINE
            using (var keyLmSoftware = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE, true))
            {
                using (var keyLmKs = keyLmSoftware.OpenSubKey(RegKeyParent, true) ?? keyLmSoftware.CreateSubKey(RegKeyParent))
                {
                    using (var keyLmEulg = keyLmKs.OpenSubKey(Branding.Registry.MachineSettingsKey, true) ?? keyLmKs.CreateSubKey(Branding.Registry.MachineSettingsKey))
                    {
                        keyLmEulg.SetValue("Install_Dir", InstallPath, RegistryValueKind.String);
                        keyLmEulg.SetValue("Version", Version, RegistryValueKind.String);
                        keyLmEulg.SetValue("Channel", Branding.Info.Channel.ToString(), RegistryValueKind.String);
                        keyLmEulg.SetValue("TerminalServer", TerminalServer ? 1 : 0, RegistryValueKind.DWord);
                        using (var memoryStream = new MemoryStream())
                        {
                            new BinaryFormatter().Serialize(memoryStream, DateTime.Now);
                            keyLmEulg.SetValue("InstallDate", memoryStream.ToArray(), RegistryValueKind.Binary);
                        }
                        WriteProfile(keyLmEulg, profile);
                    }
                }
            }
        }

        public bool InstallUpdateService()
        {
            try
            {
                if (ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
                {
                    RemoveUpdateService();
                }
                var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "xbAV Beratungssoftware GmbH", "UpdateService", "UpdateService.exe");
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

        public bool AddShellIcons(Profile profile, string urlVermittlerbereich)
        {
            try
            {
                var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), profile.DesktopApp + ".lnk");
                DeleteIfExists(file);
                file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), profile.StartMenuFolder, profile.StartMenuApp + ".lnk");
                DeleteIfExists(file);
                file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), profile.StartMenuFolder, profile.StartMenuSync + ".lnk");
                DeleteIfExists(file);

                Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), profile.StartMenuFolder));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            try
            {
                IWshShell wsh = new WshShellClass();
                var shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), profile.DesktopApp + ".lnk"));
                shtct.WindowStyle = 1; //for default window, 3 for maximize, 7 for minimize
                shtct.TargetPath = Path.Combine(InstallPath, Branding.FileSystem.AppBinary);
                shtct.IconLocation = Path.Combine(InstallPath, Branding.FileSystem.AppBinary) + ", 0";
                shtct.Save();

                var startMenuFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), profile.StartMenuFolder);

                if (!String.IsNullOrWhiteSpace(profile.StartMenuFolder) &&
                    !Directory.Exists(startMenuFolderPath))
                {
                    Directory.CreateDirectory(startMenuFolderPath);
                }

                if (!String.IsNullOrWhiteSpace(profile.StartMenuApp))
                {
                    shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                                                          profile.StartMenuFolder,
                                                                          profile.StartMenuApp + ".lnk"));
                    shtct.WindowStyle = 1;
                    shtct.TargetPath = Path.Combine(InstallPath, Branding.FileSystem.AppBinary);
                    shtct.IconLocation = Path.Combine(InstallPath, Branding.FileSystem.AppBinary) + ", 0";
                    shtct.Save();
                }

                if (!String.IsNullOrWhiteSpace(profile.StartMenuSync))
                {
                    shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                                                          profile.StartMenuFolder,
                                                                          profile.StartMenuSync + ".lnk"));
                    shtct.WindowStyle = 1;
                    shtct.TargetPath = Path.Combine(InstallPath, Branding.FileSystem.SyncBinary);
                    shtct.IconLocation = Path.Combine(InstallPath, Branding.FileSystem.SyncBinary) + ", 0";
                    shtct.Save();
                }

                if (!String.IsNullOrWhiteSpace(profile.StartMenuSupportTool))
                {
                    shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                                                          profile.StartMenuFolder,
                                                                          profile.StartMenuSupportTool + ".lnk"));
                    shtct.WindowStyle = 1;
                    shtct.TargetPath = Path.Combine(InstallPath, "Support", "EulgSupport.exe");
                    shtct.IconLocation = shtct.TargetPath + ", 0";
                    shtct.Save();
                }
                if (!String.IsNullOrWhiteSpace(profile.StartMenuFernwartung))
                {
                    shtct = (IWshShortcut)wsh.CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                                                                          profile.StartMenuFolder,
                                                                          profile.StartMenuFernwartung + ".lnk"));
                    shtct.WindowStyle = 1;
                    shtct.TargetPath = Path.Combine(InstallPath, "Support", "EulgFernwartung.exe");
                    shtct.IconLocation = shtct.TargetPath + ", 0";
                    shtct.Save();
                }
                // Link zur Webverwaltung
                if (!string.IsNullOrEmpty(urlVermittlerbereich))
                {
                    var urlWebverwLnkFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), profile.DesktopWeb + ".lnk");
                    if(!File.Exists(urlWebverwLnkFile))
                    {
                        shtct = (IWshShortcut)wsh.CreateShortcut(urlWebverwLnkFile);
                        shtct.WindowStyle = 3;
                        shtct.TargetPath = urlVermittlerbereich;
                        shtct.IconLocation = Path.Combine(InstallPath, Branding.FileSystem.SyncBinary) + ", 0";
                        shtct.Save();
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

        public bool InstallUninstaller()
        {
            try
            {
                var src = AppDomain.CurrentDomain.BaseDirectory;
                var dst = Path.Combine(InstallPath, "Setup");

                if (!Directory.Exists(dst))
                {
                    Directory.CreateDirectory(dst);
                }
                //FIXME Determine programmatically which files the setup came with, but watch out for temporary files placed in the same folder during installation
                var filesToCopyOver = new[] { "Setup.exe", "Setup.xml", "Interop.IWshRuntimeLibrary.dll", "MaterialDesignColors.dll", "MaterialDesignThemes.Wpf.dll" };
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

        public bool RegisterUninstaller(Profile profile, long estInstallSizeInKb)
        {
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                                        .OpenSubKey(SOFTWARE_UNINSTALL_REG, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
            {
                var subKey = key.OpenSubKey(Branding.Registry.MachineSettingsKey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl)
                             ?? key.CreateSubKey(Branding.Registry.MachineSettingsKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (subKey == null)
                {
                    throw new Exception("Kann Uninstall nicht schreiben!");
                }

                var setupExe = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

                subKey.SetValue("DisplayName", profile.DesktopApp, RegistryValueKind.String);
                subKey.SetValue("DisplayIcon", Path.Combine(InstallPath, "Setup", setupExe), RegistryValueKind.String);
                subKey.SetValue("Publisher", "xbAV Beratungssoftware GmbH", RegistryValueKind.String);
                subKey.SetValue("URLInfoAbout", "www.xbav-berater.de", RegistryValueKind.String);
                subKey.SetValue("UninstallString", Path.Combine(InstallPath, "Setup", setupExe) + " /U", RegistryValueKind.String);
                subKey.SetValue("InstallDate", $"{DateTime.Now:YYYYMMDD}", RegistryValueKind.String);
                subKey.SetValue("InstallLocation", InstallPath, RegistryValueKind.String);
                subKey.SetValue("EstimatedSize", estInstallSizeInKb, RegistryValueKind.DWord);
                subKey.SetValue("Language", 1033, RegistryValueKind.DWord);
                subKey.SetValue("NoModify", 0, RegistryValueKind.DWord);
                subKey.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                subKey.SetValue("ModifyPath", Path.Combine(InstallPath, "Support", "EulgSupport.exe"), RegistryValueKind.String);

                var tmpVersion = Version;
                if (!String.IsNullOrWhiteSpace(Branding.Info.BuildTag))
                {
                    tmpVersion += " (" + Branding.Info.BuildTag + ")";
                }
                if (TerminalServer)
                {
                    tmpVersion += " TerminalServer";
                }
                subKey.SetValue("DisplayVersion", tmpVersion, RegistryValueKind.String);
                Version v;
                if (System.Version.TryParse(Version, out v))
                {
                    subKey.SetValue("Version", v.Build, RegistryValueKind.DWord);
                    subKey.SetValue("VersionMajor", v.Major, RegistryValueKind.DWord);
                    subKey.SetValue("VersionMinor", v.Minor, RegistryValueKind.DWord);
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
                    var setupExe = Assembly.GetExecutingAssembly().Location;
                    var keyRunOnce = key.OpenSubKey("RunOnce", true) ?? key.CreateSubKey("RunOnce");
                    keyRunOnce.SetValue("xbAVSetup", $"\"{setupExe}\" /D");
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
                    keyRunOnce.DeleteValue("xbAVSetup", false);
                }
            }
            catch
            {
            }
        }

        public bool WriteBranding()
        {
            try
            {
                Branding.Write(Path.Combine(InstallPath, "Branding.xml"));
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
            return false;
        }

        private static void WriteProfile(RegistryKey keyLmEulg, Profile profile)
        {
            using (var buffer = new MemoryStream())
            {
                using (var deflate = new DeflateStream(buffer, CompressionMode.Compress, true))
                {
                    new XmlSerializer(typeof(Profile)).Serialize(deflate, profile);
                }

                keyLmEulg.SetValue("Profile", buffer.ToArray(), RegistryValueKind.Binary);
            }
        }

        public static string ReadInstalledVersion(string machineSettingsKey)
        {
            var parentKeys = new[] { REGISTRY_GROUP_NAME_KS, REGISTRY_GROUP_NAME_EULG, REGISTRY_GROUP_NAME };

            using(var keyLmSoftware = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE))
            {
                foreach (var parentKey in parentKeys)
                {
                    using(var profileKey = keyLmSoftware.OpenSubKey($"{parentKey}\\{machineSettingsKey}", false))
                    {
                        if (profileKey != null)
                        {
                            return profileKey.GetValue("Version").ToString();
                        }
                    }
                }
            }

            return null;
        }

        public Profile ReadInstalledProfile()
        {
            using(var keyLmSoftware = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(REG_KEY_SOFTWARE))
            {
                using(var keyLmKs = keyLmSoftware.OpenSubKey(RegKeyParent, false))
                {
                    using(var keyLmEulg = keyLmKs.OpenSubKey(Branding.Registry.MachineSettingsKey, false))
                    {
                        var bytes = (byte[])keyLmEulg?.GetValue("Profile");
                        if (bytes == null)
                        {
                            if (new Version(Version).Major < 2)
                            {
                                return new Profile //TODO legacy profile for updated pre-2.0 installations
                                {
                                    DesktopApp = "EULG-Client",
                                    DesktopWeb = "Vermittlerbereich",
                                    StartMenuApp = "EULG-Client",
                                    StartMenuFernwartung = "Fernwartung",
                                    StartMenuFolder = "EULG Software GmbH",
                                    StartMenuSupportTool = "Support-Tool",
                                    StartMenuSync = "EULG Sync-Client"
                                };
                            }

                            return null;
                        }

                        using(var buffer = new MemoryStream(bytes))
                        {
                            using(var inflate = new DeflateStream(buffer, CompressionMode.Decompress))
                            {
                                return (Profile)new XmlSerializer(typeof(Profile)).Deserialize(inflate);
                            }
                        }
                    }
                }
            }
        }

        public bool InstallNgen()
        {
            try
            {
                var ngenBinary = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
                RunProcess(ngenBinary, "queue pause", InstallPath);
                if (!string.IsNullOrWhiteSpace(Branding.FileSystem.AppBinary))
                {
                    RunProcess(ngenBinary, $"install \"{Path.Combine(InstallPath, Branding.FileSystem.AppBinary)}\" /queue:1", InstallPath);
                }
                if (!string.IsNullOrWhiteSpace(Branding.FileSystem.SyncBinary))
                {
                    RunProcess(ngenBinary, $"install \"{Path.Combine(InstallPath, Branding.FileSystem.SyncBinary)}\" /queue:1", InstallPath);
                }
                RunProcess(ngenBinary, "queue continue", InstallPath);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            return true;
        }

        public void ExtractAppDir()
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
            var dstPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "xbAV Beratungssoftware GmbH", "UpdateService");
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

        public bool RemoveUpdateService()
        {
            if (!ServiceController.GetServices().Any(a => a.ServiceName.Equals(UPDATE_SERVICE_NAME)))
            {
                return true;
            }
            var exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "xbAV Beratungssoftware GmbH", "UpdateService", "UpdateService.exe");
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

        public static bool RemoveShellIcons(Profile profile)
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), profile.DesktopApp + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), profile.DesktopWeb + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), profile.StartMenuFolder, profile.StartMenuApp + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), profile.StartMenuFolder, profile.StartMenuSync + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), profile.StartMenuFolder, profile.StartMenuSupportTool + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), profile.StartMenuFolder, profile.StartMenuFernwartung + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), profile.DesktopApp + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), profile.DesktopWeb + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), profile.StartMenuFolder, profile.StartMenuApp + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), profile.StartMenuFolder, profile.StartMenuSync + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), profile.StartMenuFolder, profile.StartMenuSupportTool + ".lnk");
            DeleteIfExists(file);
            file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), profile.StartMenuFolder, profile.StartMenuFernwartung + ".lnk");
            DeleteIfExists(file);

            try
            {
                Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), profile.StartMenuFolder));
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
            try
            {
                Directory.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), profile.StartMenuFolder));
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

        public bool ClearAppDir()
        {
            DelTree(InstallPath);
            return true;
        }

        public bool ClearReg()
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
                            keyLmKs?.DeleteSubKeyTree(Branding.Registry.MachineSettingsKey, false);
                        }
                        using (var keyLmKs = keyLmSoftware.OpenSubKey(REGISTRY_GROUP_NAME_EULG, true))
                        {
                            keyLmKs?.DeleteSubKeyTree(Branding.Registry.MachineSettingsKey, false);
                        }
                        using (var keyLmKs = keyLmSoftware.OpenSubKey(REGISTRY_GROUP_NAME_KS, true))
                        {
                            keyLmKs?.DeleteSubKeyTree(Branding.Registry.MachineSettingsKey, false);
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
                            keyKs?.DeleteSubKeyTree(Branding.Registry.UserSettingsKey, false);
                        }
                        using (var keyKs = keySoftware.OpenSubKey(REGISTRY_GROUP_NAME_EULG, true))
                        {
                            keyKs?.DeleteSubKeyTree(Branding.Registry.UserSettingsKey, false);
                        }
                        using (var keyKs = keySoftware.OpenSubKey(REGISTRY_GROUP_NAME_KS, true))
                        {
                            keyKs?.DeleteSubKeyTree(Branding.Registry.UserSettingsKey, false);
                        }
                    }
                }

                using (var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(SOFTWARE_CURR_VER_REG, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
                {
                    if (key != null)
                    {
                        using (var keyRunOnce = key.OpenSubKey("Run", true) ?? key.CreateSubKey("Run"))
                        {
                            keyRunOnce?.DeleteValue(Branding.Registry.UserSettingsKey + "-Sync", false);
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

        public bool UnregisterUninstall()
        {
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                                        .OpenSubKey(SOFTWARE_UNINSTALL_REG, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl))
            {
                if (key?.OpenSubKey(Branding.Registry.MachineSettingsKey) != null)
                {
                    key.DeleteSubKeyTree(Branding.Registry.MachineSettingsKey);
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

            var brandingSrc = Path.GetFullPath(Path.Combine(src, "..", "Branding.xml"));
            var brandingDst = Path.Combine(tmpDir, "Branding.xml");
            File.Copy(brandingSrc, brandingDst);
        }

        public bool UninstallNgen()
        {
            try
            {
                var ngenBinary = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "ngen.exe");
                if (!string.IsNullOrWhiteSpace(Branding.FileSystem.AppBinary))
                {
                    RunProcess(ngenBinary, $"uninstall \"{Path.Combine(InstallPath, Branding.FileSystem.AppBinary)}\"", InstallPath);
                }
                if (!string.IsNullOrWhiteSpace(Branding.FileSystem.SyncBinary))
                {
                    RunProcess(ngenBinary, $"uninstall \"{Path.Combine(InstallPath, Branding.FileSystem.SyncBinary)}\"", InstallPath);
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

        public void Log(UpdateClient.ELogTypeEnum logType, string message)
        {
            UpdateClient.Log(logType, message);
        }

        public void LogException(Exception exception)
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

        private static byte[] EncryptPassword(string str)
        {
            return ProtectedData.Protect(Encoding.UTF8.GetBytes(str), null, DataProtectionScope.CurrentUser);
        }

        private int RunProcess(string fileName, string arguments, string workingDirectory)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                process.Start();
                process.WaitForExit();
                var err = process.StandardError.ReadToEnd();

                if(!String.IsNullOrWhiteSpace(err))
                {
                    Log(UpdateClient.ELogTypeEnum.Warning, err);
                }

                return process.ExitCode;
            }
        }

        public bool TryGetApi(EApiResource resource, out Uri uri, bool failQuietly = false)
        {
            return TryGetApi(Config.ApiManifestUri, Config.Channel, resource, out uri, failQuietly);
        }

        public static bool TryGetApi(string serviceUrl, Branding.EUpdateChannel channel, EApiResource resource, out Uri uri, bool failQuietly = false)
        {
            if(_apiManifest == null)
            {
                var apiClient = new ApiResourceClient(serviceUrl, channel);

                try
                {
                    _apiManifest = apiClient.Fetch();
                }
                catch(Exception ex)
                {
                    if (!failQuietly)
                    {
                        var message = "Die Schnittstelle des zuständigen Serverdienstes für die gewählte Funktion konnte nicht ermittelt werden. Eine Internetverbindung wird benötigt. Systemmeldung:"
                                      + Environment.NewLine + Environment.NewLine
                                      + ex.GetMessagesTree();
                        Action action = delegate
                        {
                            var window = Application.Current.MainWindow;
                            if (window == null)
                            {
                                MessageBox.Show(message, "Kommunikation mit Serverdient nicht möglich", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                            else
                            {
                                MessageBox.Show(window, message, "Kommunikation mit Serverdient nicht möglich", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        };
                        Application.Current.Dispatcher.Invoke(action);
                    }

                    uri = null;
                    return false;
                }
            }

            return _apiManifest.TryGetValue(resource, out uri);
        }

        public static Uri GetUpdateApi(string serviceUri, Branding.EUpdateChannel channel, string method, bool forceInsecure = false)
        {
            Uri update;
            if (!TryGetApi(serviceUri, channel, EApiResource.UpdateService, out update))
            {
                return null;
            }

            var builder = new UriBuilder(update + method);
            if(forceInsecure)
            {
                builder.Scheme = "http";
            }
            return builder.Uri;
        }

        #endregion
    }
}
