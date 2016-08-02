using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Eulg.Shared;

namespace Update.Fix.Fixes
{
    internal class BrandingEntries : FixBase, IFix
    {
        private BrandingEntries() { }

        public static IFix Inst { get; } = new BrandingEntries();

        public string Name => nameof(BrandingEntries);

        public bool? Check()
        {
            return DoIt(false);
        }

        public void Apply()
        {
            DoIt(true);
        }

        private static bool DoIt(bool fix)
        {
            var brandingXmlFile = GetBrandingFileName();

            Branding branding;
            using(var fileStream = File.OpenRead(brandingXmlFile))
            {
                var xmlSerializer = new XmlSerializer(typeof(Branding));
                branding = (Branding)xmlSerializer.Deserialize(fileStream);
            }

            if (branding.Version >= 2)
            {
                return true;
            }

            branding.Version = 2;

            var updateUrl = LoadBranding().SelectSingleNode("/Branding/Urls")?.Attributes?["Update"]?.Value;
            if (updateUrl == null)
            {
                throw new Exception("Kann Update-URL nicht bestimmen");
            }

            if(!updateUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                updateUrl = $"http://{updateUrl}";
            }

            updateUrl = updateUrl.Replace("/service.entgeltumwandler.de/", "/service.xbav-berater.de/")
                                 .Replace("/service.eulg.de/", "/service.xbav-berater.de/");

            var manifestUrl = Regex.Replace(updateUrl, @"\/update\/?$", "/ApiManifest/JsonGet", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            if(manifestUrl != updateUrl)
            {
                branding.Info.ApiManifestUri = manifestUrl;
            }

            if (!fix)
            {
                return false;
            }

            var backupFilename = Path.ChangeExtension(brandingXmlFile, ".xml.bak");
            var counter = 0;
            while (File.Exists(backupFilename))
            {
                backupFilename = Path.ChangeExtension(brandingXmlFile, $".xml.{++counter}.bak");
            }

            File.Copy(brandingXmlFile, backupFilename);

            using(var fileStream = File.OpenWrite(brandingXmlFile))
            {
                var xmlSerializer = new XmlSerializer(typeof(Branding));
                xmlSerializer.Serialize(fileStream, branding);
            }

            return true;
        }
    }
}
