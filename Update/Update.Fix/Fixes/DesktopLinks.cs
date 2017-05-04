using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Update.Fix.Fixes
{
    internal class DesktopLinks : LinksBase, IFix
    {
        private const string LINK_NAME = "xbAV-Berater.lnk";

        private static readonly string _desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private static readonly string _commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
        private static readonly string _taskBar = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");

        private DesktopLinks() { }

        public static IFix Inst { get; } = new DesktopLinks();

        public string Name => nameof(DesktopLinks);

        public bool? Check()
        {
            var linksToCheck = Directory.GetFiles(_desktop, "*.lnk")
                                        .Concat(Directory.GetFiles(_commonDesktop, "*.lnk"))
                                        .Concat(Directory.GetFiles(_taskBar, "*.lnk"));

            var executable = GetClientExecutablePath();
            foreach (var link in linksToCheck)
            {
                if (!string.Equals(Path.GetFileName(link), LINK_NAME, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(GetShortcutTargetFile(link), executable, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public void Apply()
        {
            var linksToCheck = Directory.GetFiles(_desktop, "*.lnk")
                                        .Concat(Directory.GetFiles(_commonDesktop, "*.lnk"))
                                        .Concat(Directory.GetFiles(_taskBar, "*.lnk"));

            var executable = GetClientExecutablePath();
            var haveNewShortcuts = false;

            foreach(var link in linksToCheck)
            {
                try
                {
                    if (string.Equals(GetShortcutTargetFile(link), executable, StringComparison.OrdinalIgnoreCase))
                    {
                        var newlink = Path.Combine(Path.GetDirectoryName(link), LINK_NAME);

                        File.Delete(link);
                        SetLink(newlink, executable);
                        haveNewShortcuts = true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception {e}");
                    // ignore
                }
            }

            try
            {
                if (haveNewShortcuts && Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    var version = Environment.OSVersion.Version;
                    if(version.Major == 6 && version.Minor < 3) Process.Start("ie4uinit.exe", "-ClearIconCache")?.Dispose();
                    if(version.Major == 10) Process.Start("ie4uinit.exe", "-show")?.Dispose();
                }
            }
            catch
            {
                // ignore
            }
        }

        private string GetClientExecutablePath()
        {
            return Path.Combine(GetInstallDir(), CLIENT);
        }

        private static string GetShortcutTargetFile(string shortcutFilename)
        {
            WshShell shell = new WshShell();
            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutFilename);

            return link.TargetPath;
        }
    }
}
