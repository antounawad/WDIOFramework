using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eulg.Shared;
using IWshRuntimeLibrary;
using Shell32;
using File = System.IO.File;
using Folder = IWshRuntimeLibrary.Folder;

namespace Update.Fix.Fixes
{
    public class DesktopLinks
    {
        private const string BRANDING_FILE_NAME = "Branding.xml";

        private const string CLIENT = "EULG_client.exe";

        private static Branding _branding;
        private static Branding Branding
        {
            get
            {
                if (_branding == null)
                {
                    var brandingXmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BRANDING_FILE_NAME);
                    if (!File.Exists(brandingXmlFile)) throw new Exception("Datei " + brandingXmlFile + " nicht gefunden.");

                    _branding = Branding.Read(brandingXmlFile);
                }

                return _branding;
            }
        }

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

        private static void SetLink(string link, string destination)
        {
            var shell = new WshShellClass();

            try
            {
                var shortcut = (IWshShortcut)shell.CreateShortcut(link);
                shortcut.TargetPath = destination;
                shortcut.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
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
