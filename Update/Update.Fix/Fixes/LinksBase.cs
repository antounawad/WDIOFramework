using System;
using System.IO;
using Eulg.Shared;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Update.Fix.Fixes
{
    public class LinksBase
    {
        private const string BRANDING_FILE_NAME = "Branding.xml";

        protected const string CLIENT = "EULG_client.exe";

        private static Branding _branding;
        protected static Branding Branding
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

        protected static void SetLink(string link, string destination)
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
