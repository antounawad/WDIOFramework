using System;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Update.Fix.Fixes
{
    internal class DesktopLinks : LinksBase, IFix
    {
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
                if (string.Equals(GetShortcutTargetFile(link), executable, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
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
            foreach(var link in linksToCheck)
            {
                try
                {
                    if (string.Equals(GetShortcutTargetFile(link), executable, StringComparison.OrdinalIgnoreCase))
                    {
                        var newlink = Path.Combine(Path.GetDirectoryName(link), "xbAV-Berater.lnk");

                        File.Delete(link);
                        SetLink(newlink, executable);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception {e}");
                    // ignore
                }
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
