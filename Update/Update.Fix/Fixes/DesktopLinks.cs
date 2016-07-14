using System;
using System.IO;
using System.Linq;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Update.Fix.Fixes
{
    public class DesktopLinks: LinksBase
    {
        internal static bool Check()
        {
            if (Branding != null)
            {
                var eulgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CLIENT);

                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                var commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);

                var linksToCheck = Directory.GetFiles(desktop, "*.lnk").Concat(Directory.GetFiles(commonDesktop, "*.lnk"));

                foreach(var link in linksToCheck)
                {
                    if(GetShortcutTargetFile(link) == eulgPath)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        internal static void Fix()
        {
            if (Branding != null)
            {
                var eulgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CLIENT);

                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                var commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);

                var linksToCheck = Directory.GetFiles(desktop, "*.lnk").Concat(Directory.GetFiles(commonDesktop, "*.lnk"));

                foreach (var link in linksToCheck)
                {
                    if (GetShortcutTargetFile(link) == eulgPath)
                    {
                        var newlink = Path.Combine(Path.GetDirectoryName(link), Branding.ShellIcons.DesktopApp + ".lnk");

                        File.Delete(link);
                        SetLink(newlink, eulgPath);
                    }
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
