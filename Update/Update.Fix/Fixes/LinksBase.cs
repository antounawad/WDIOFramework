using System;
using IWshRuntimeLibrary;

namespace Update.Fix.Fixes
{
    internal abstract class LinksBase : FixBase
    {
        protected const string CLIENT = "EULG_client.exe";

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
