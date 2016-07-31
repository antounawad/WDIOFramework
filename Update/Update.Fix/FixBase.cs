using System;
using System.IO;
using System.Xml;

namespace Update.Fix
{
    internal abstract class FixBase
    {
        public const string BRANDING_FILENAME = "Branding.xml";

        protected static string GetBuildTag()
        {
            var branding = LoadBranding();
            return branding?.SelectSingleNode("/Branding/Info")?.Attributes?["BuildTag"]?.Value;
        }

        protected static string GetHkcuProfileName()
        {
            var branding = LoadBranding();
            return branding?.SelectSingleNode("/Branding/Registry")?.Attributes?["UserSettingsKey"]?.Value;
        }

        protected static string GetHklmProfileName()
        {
            var branding = LoadBranding();
            return branding?.SelectSingleNode("/Branding/Registry")?.Attributes?["MachineSettingsKey"]?.Value;
        }

        protected static string GetInstallDir()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(path))
            {
                // Where the branding file is, or at least where it should be
                if (File.Exists(Path.Combine(path, BRANDING_FILENAME)) || File.Exists(Path.Combine(path, "EULG_client.exe")))
                {
                    return path;
                }
            }

            return null;
        }

        protected static string GetBrandingFileName()
        {
            var installDir = GetInstallDir();
            return installDir == null ? null : Path.Combine(GetInstallDir(), BRANDING_FILENAME);
        }

        /// <summary>
        /// Serialisierungsunabhängiger Zugriff auf die Branding, für Fixes die für mehrere Branding-Versionen anwendbar sein sollen.
        /// </summary>
        protected static XmlDocument LoadBranding()
        {
            var filename = GetBrandingFileName();
            if (filename == null)
            {
                return null;
            }

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filename);
            return xmlDocument;
        }
    }
}
