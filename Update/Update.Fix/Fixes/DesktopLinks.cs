using System;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Update.Fix.Fixes
{
    public class DesktopLinks: LinksBase
    {
        private static readonly string _eulgPath = Path.GetFullPath(Path.Combine(BASEDIRECTORY, CLIENT));
        private static readonly string _desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private static readonly string _commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
        private static readonly string _taskBar = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");

        public static bool Check()
        {
            var linksToCheck = Directory.GetFiles(_desktop, "*.lnk")
                                .Concat(Directory.GetFiles(_commonDesktop, "*.lnk"))
                                .Concat(Directory.GetFiles(_taskBar, "*.lnk"));

            foreach (var link in linksToCheck)
            {
                if (GetShortcutTargetFile(link) == _eulgPath)
                {
                    return false;
                }
            }

            return true;
        }

        public static void Fix()
        {
            var linksToCheck = Directory.GetFiles(_desktop, "*.lnk")
                                .Concat(Directory.GetFiles(_commonDesktop, "*.lnk"))
                                .Concat(Directory.GetFiles(_taskBar, "*.lnk"));

            foreach (var link in linksToCheck)
            {
                try
                {
                    var targetlink = GetShortcutTargetFile(link);

                    if (targetlink == _eulgPath)
                    {
                        var newlink = Path.Combine(Path.GetDirectoryName(link), "xbAV Berater.lnk");

                        File.Delete(link);
                        SetLink(newlink, _eulgPath);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception {e}");
                    // ignore
                }
            }
        }

        private static string GetShortcutTargetFile(string shortcutFilename)
        {
            WshShell shell = new WshShell();
            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(shortcutFilename);

            return link.TargetPath;
        }
    }
}
