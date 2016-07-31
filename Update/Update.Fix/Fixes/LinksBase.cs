using System;
using System.IO;
using IWshRuntimeLibrary;

namespace Update.Fix.Fixes
{
    public class LinksBase
    {
        protected const string CLIENT = "EULG_client.exe";

        protected static readonly string BASEDIRECTORY = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..");

        protected static readonly string BASEDIRECTORYNAME = Path.GetFileName(Path.GetFullPath(BASEDIRECTORY).TrimEnd(Path.DirectorySeparatorChar));

        public static void SetLink(string link, string destination)
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
    }
}
