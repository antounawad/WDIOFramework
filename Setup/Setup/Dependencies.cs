using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using Microsoft.Win32;

namespace Eulg.Setup
{
    public static class Dependencies
    {
        public static bool NeedsJre = false;

        //public static Version VersionWinVista = new Version("6.0.6000");
        public static Version VersionWin7 = new Version("6.1.7600");
        public static Version VersionWin8 = new Version("6.2.9200");
        //public static Version VersionWin81 = new Version("6.3.9600");
        //public static Version VersionWin10 = new Version("6.2.9200"); CRAP!

        // ReSharper disable InconsistentNaming
        //public const int REL_NET45 = 378389;
        public const int REL_NET451_ON_WIN81 = 378675;
        //public const int REL_NET451_ON_WIN_OLD_WIN = 378758;
        //public const int REL_NET452 = 379893;
        //public const int REL_NET46_ON_WIN10 = 393295;
        //public const int REL_NET46_ON_WIN_OLD_WIN = 393297;
        //public const int REL_NET461_ON_WIN10 = 394254;
        //public const int REL_NET461_ON_WIN_OLD_WIN = 394271;
        //public const int REL_NET462_ON_WIN10 = 394802;
        //public const int REL_NET462_ON_WIN_OLD_WIN = 394806;
        //public const int REL_NET47_ON_WIN10 = 460798;
        //public const int REL_NET47_ON_WIN_OLD_WIN = 460805;
        //public const int REL_NET471_ON_WIN10 = 461308;
        //public const int REL_NET471_ON_WIN_OLD_WIN = 461310;

        public const string DOWNLOAD_URL_NET471 = @"https://download.microsoft.com/download/8/E/2/8E2BDDE7-F06E-44CC-A145-56C6B9BBE5DD/NDP471-KB4033344-Web.exe";
        //public const string DOWNLOAD_URL_NET47 = @"https://download.microsoft.com/download/A/E/A/AEAE0F3F-96E9-4711-AADA-5E35EF902306/NDP47-KB3186500-Web.exe";
        //public const string DOWNLOAD_URL_NET461 = @"https://download.microsoft.com/download/3/5/9/35980F81-60F4-4DE3-88FC-8F962B97253B/NDP461-KB3102438-Web.exe";
        //public const string DOWNLOAD_URL_NET46 = @"http://download.microsoft.com/download/1/4/A/14A6C422-0D3C-4811-A31F-5EF91A83C368/NDP46-KB3045560-Web.exe";
        //public const string DOWNLOAD_URL_NET452 = @"http://download.microsoft.com/download/B/4/1/B4119C11-0423-477B-80EE-7A474314B347/NDP452-KB2901954-Web.exe";
        //public const string DOWNLOAD_URL_NET451 = @"http://download.microsoft.com/download/7/4/0/74078A56-A3A1-492D-BBA9-865684B83C1B/NDP451-KB2859818-Web.exe";
        public const string DOWNLOAD_URL_CPP2008 = @"http://download.microsoft.com/download/5/D/8/5D8C65CB-C849-4025-8E95-C3966CAFD8AE/vcredist_x86.exe";
        public const string DOWNLOAD_URL_CPP2012 = @"http://download.microsoft.com/download/A/3/7/A371A2C7-B787-4AD9-B56D-8319CE7B40CA/VSU4/vcredist_x86.exe";
        public const string DOWNLOAD_URL_IE9_WIN7_X64 = @"http://download.microsoft.com/download/B/B/B/BBBB0466-AE6E-46B9-AFE8-523A6C9E4232/IE9-Windows7-x64-deu.exe";
        public const string DOWNLOAD_URL_IE9_WIN7_X86 = @"http://download.microsoft.com/download/F/6/4/F6414410-F454-43BA-834E-1B4A7C1E774C/IE9-Windows7-x86-deu.exe";
        public const string DOWNLOAD_URL_IE9_WIN_VISTAX64 = @"http://download.microsoft.com/download/8/3/2/83205D42-C4DE-435E-AF10-4919CBDB3A13/IE9-WindowsVista-x64-deu.exe";
        public const string DOWNLOAD_URL_IE9_WIN_VISTAX86 = @"http://download.microsoft.com/download/1/E/9/1E9DE3C7-0C84-41C8-BBED-997EB0C98CCA/IE9-WindowsVista-x86-deu.exe";
        public const string DOWNLOAD_URL_JRE = @"http://javadl.sun.com/webapps/download/AutoDL?BundleId=81870";
        // ReSharper restore InconsistentNaming

        public enum EDependencyState
        {
            [Description("vorhanden")]
            Installed,
            [Description("fehlt")]
            Missing,
            [Description("übersprungen")]
            Skipped,
            [Description("unbekannt")]
            Error,
        }

        public static EDependencyState Net451 { get; set; }
        public static EDependencyState Cpp2008 { get; set; }
        public static EDependencyState Cpp2012 { get; set; }
        public static EDependencyState Ie9 { get; set; }
        public static EDependencyState Jre { get; set; }

        public static bool CheckAll()
        {
            Net451 = CheckNet451();
            Cpp2008 = CheckCpp2008();
            Cpp2012 = CheckCpp2012();
            Ie9 = CheckIe9();
            if (NeedsJre)
            {
                Jre = CheckJre();
            }

            if (!Net451.IsOk())
            {
                return false;
            }
            if (!Cpp2008.IsOk())
            {
                return false;
            }
            if (!Cpp2012.IsOk())
            {
                return false;
            }
            if (!Ie9.IsOk())
            {
                return false;
            }
            if (NeedsJre)
            {
                if (!Jre.IsOk())
                {
                    return false;
                }
            }

            return true;
        }

        private static void LogException(Exception exception)
        {
            MessageBox.Show(exception.Message);
        }

        public static bool IsOk(this EDependencyState eDependencyState)
        {
            return (eDependencyState == EDependencyState.Installed || eDependencyState == EDependencyState.Skipped);
        }

        public static EDependencyState CheckNet451()
        {
            try
            {
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full"))
                {
                    if (key != null)
                    {
                        var ver = key.GetValue("Release") as int?;
                        var inst = key.GetValue("Install") as int?;
                        if (ver != null && inst != null)
                        {
                            if (ver >= REL_NET451_ON_WIN81 && inst > 0)
                            {
                                return EDependencyState.Installed;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
                return EDependencyState.Error;
            }
            return EDependencyState.Missing;
        }

        public static EDependencyState CheckCpp2008()
        {
            try
            {
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
                {
                    if (key != null)
                    {
                        string[] x = key.GetSubKeyNames();
                        if (x.Contains("{1F1C2DFC-2D24-3E06-BCB8-725134ADF989}"))
                        {
                            return EDependencyState.Installed; // Das ist die die wir installieren!
                        }

                        if (x.Contains("{9A25302D-30C0-39D9-BD6F-21E6EC160475}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{86CE1746-9EFF-3C9C-8755-81EA8903AC34}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{CA8A885F-E95B-3FC6-BB91-F4D9377C7686}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{820B6609-4C97-3A2B-B644-573B06A0F0CC}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{6AFCA4E1-9B78-3640-8F72-A7BF33448200}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{F03CB3EF-DC16-35CE-B3C1-C68EA09E5E97}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{402ED4A1-8F5B-387A-8688-997ABF58B8F2}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{887868A2-D6DE-3255-AA92-AA0B5A59B874}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{527BBE2F-1FED-3D8B-91CB-4DB0F838E69E}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{57660847-B1F7-35BD-9118-F62EB863A598}"))
                        {
                            return EDependencyState.Installed;
                        }
                        if (x.Contains("{9BE518E6-ECC6-35A9-88E4-87755C07200F}"))
                        {
                            return EDependencyState.Installed;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
                return EDependencyState.Error;
            }
            return EDependencyState.Missing;
        }

        public static EDependencyState CheckCpp2012()
        {
            try
            {
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\DevDiv\VC\Servicing\11.0\RuntimeMinimum"))
                {
                    var ver = key?.GetValue("Version") as string;
                    var updateVersion = key?.GetValue("UpdateVersion") as string;
                    if (ver != null)
                    {
                        var vIs = new Version(ver);
                        var vShould = new Version("11.0.61030");
                        var vUpdate = new Version(updateVersion ?? "1.0.0");
                        if (vIs >= vShould || vUpdate >= vShould)
                        {
                            return EDependencyState.Installed;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
                return EDependencyState.Error;
            }
            return EDependencyState.Missing;
        }

        public static EDependencyState CheckCpp2013()
        {
            try
            {
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\DevDiv\VC\Servicing\12.0\RuntimeMinimum"))
                {
                    var ver = key?.GetValue("Version") as string;
                    var updateVersion = key?.GetValue("UpdateVersion") as string;
                    if (ver != null)
                    {
                        var vIs = new Version(ver);
                        var vShould = new Version("12.0.21005");
                        var vUpdate = new Version(updateVersion ?? "1.0.0");
                        if (vIs >= vShould || vUpdate >= vShould)
                        {
                            return EDependencyState.Installed;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
                return EDependencyState.Error;
            }
            return EDependencyState.Missing;
        }

        public static EDependencyState CheckIe9()
        {
            try
            {
                if (Environment.OSVersion.Version >= VersionWin8)
                {
                    return EDependencyState.Skipped;
                }
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer"))
                {
                    var ver = key?.GetValue("Version") as string;
                    if (ver != null)
                    {
                        var vIs = new Version(ver);
                        var vShould = new Version("9.0");
                        if (vIs >= vShould)
                        {
                            return EDependencyState.Installed;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
                return EDependencyState.Error;
            }
            return EDependencyState.Missing;
        }

        public static EDependencyState CheckJre()
        {
            try
            {
                using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment"))
                {
                    if (key != null)
                    {
                        string[] x = key.GetSubKeyNames();
                        var vShould = new Version("1.7");
                        foreach (var s in x)
                        {
                            try
                            {
                                var vIs = new Version(s);
                                if (vIs >= vShould)
                                {
                                    return EDependencyState.Installed;
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
                return EDependencyState.Error;
            }
            return EDependencyState.Missing;
        }

        public static bool InstallNet47()
        {
            const string SETUP_FILE_NAME = "NDP47-KB3186500-Web.exe";
            const string ARGUMENTS = "/passive /LCID 1031";

            DoInstall(SETUP_FILE_NAME, ARGUMENTS, DOWNLOAD_URL_NET471, ".NET Framework 4.71");

            return CheckNet451().IsOk();
        }

        public static bool InstallCpp2008()
        {
            const string SETUP_FILE_NAME = "vcredist_2008_x86.exe";
            const string ARGUMENTS = "/q";

            DoInstall(SETUP_FILE_NAME, ARGUMENTS, DOWNLOAD_URL_CPP2008, "Visual C++ 2008 SP1 Runtime");

            return CheckCpp2008().IsOk();
        }

        public static bool InstallCpp2012()
        {
            const string SETUP_FILE_NAME = "vcredist_2012_x86.exe";
            const string ARGUMENTS = "/passive";

            DoInstall(SETUP_FILE_NAME, ARGUMENTS, DOWNLOAD_URL_CPP2012, "Visual C++ 2012 Update 4 Runtime");

            return CheckCpp2012().IsOk();
        }

        public static bool InstallIe9()
        {
            const string ARGUMENTS = "/passive";
            const string NAME = "Internet Explorer 9";
            if (Environment.OSVersion.Version >= VersionWin7)
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    DoInstall("IE9-Windows7-x64-deu.exe", ARGUMENTS, DOWNLOAD_URL_IE9_WIN7_X64, NAME);
                }
                else
                {
                    DoInstall("IE9-Windows7-x86-deu.exe", ARGUMENTS, DOWNLOAD_URL_IE9_WIN7_X86, NAME);
                }
            }
            else
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    DoInstall("IE9-WindowsVista-x64-deu.exe", ARGUMENTS, DOWNLOAD_URL_IE9_WIN_VISTAX64, NAME);
                }
                else
                {
                    DoInstall("IE9-WindowsVista-x86-deu.exe", ARGUMENTS, DOWNLOAD_URL_IE9_WIN_VISTAX86, NAME);
                }
            }
            return CheckIe9().IsOk();
        }

        public static bool InstallJre()
        {
            const string SETUP_FILE_NAME = "jxpiinstall.exe";
            const string ARGUMENTS = "/s";
            const string DOWNLOAD_URL = DOWNLOAD_URL_JRE;

            DoInstall(SETUP_FILE_NAME, ARGUMENTS, DOWNLOAD_URL, "Java Runtime Environment 1.7");

            return CheckJre().IsOk();
        }

        private static void DoInstall(string setupFileName, string arguments, string downloadUrl, string name)
        {
            SetupHelper.ReportProgress(name, "Download...", -1);

            var setupFile = Path.Combine(Path.GetTempPath(), setupFileName);
            var wc = new WebClient();
#if DEBUG
            System.Threading.Thread.Sleep(500);
#else
            wc.DownloadFile(downloadUrl, setupFile);
#endif
            var p = new Process
            {
                StartInfo =
                {
                    FileName = setupFile,
                    Arguments = arguments
                }
            };
            SetupHelper.ReportProgress(name, "Installation...", -1);
#if DEBUG
            MessageBox.Show(p.StartInfo.FileName + " " + p.StartInfo.Arguments);
#else
            p.Start();
            p.WaitForExit();
#endif
        }
    }
}
