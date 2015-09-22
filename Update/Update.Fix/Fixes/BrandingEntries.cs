using System;
using System.IO;
using System.Xml;

namespace Update.Fix.Fixes
{
    internal static class BrandingEntries
    {
        private const string API_MANIFEST_ATTRIBUTE = "ApiManifest";
        private const string BRANDING_FILE_NAME = "Branding.xml";

        internal static bool Check()
        {
            try
            {
                return DoIt(false);
            }
            catch
            {
                return false;
            }
        }

        internal static void Fix()
        {
            DoIt(true);
        }

        private static bool DoIt(bool fix)
        {
            var brandingXmlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BRANDING_FILE_NAME);
            if (!File.Exists(brandingXmlFile)) throw new Exception("Datei " + brandingXmlFile + " nicht gefunden.");
            var doc = new XmlDocument();
            doc.Load(brandingXmlFile);

            var node = doc.SelectSingleNode("/Branding/Urls");
            var update_attr = node.Attributes["Update"];

            var newApi = "http://service.eulg.de";
            if (update_attr.Value.IndexOf("service.eulg.de", StringComparison.InvariantCultureIgnoreCase) >= 0
             || update_attr.Value.IndexOf("service.entgeltumwandler.de", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                newApi = "http://service.eulg.de";
            }
            else if (update_attr.Value.IndexOf("test.eulg.de", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                newApi = "http://test.eulg.de/Service";
            }
            else if (update_attr.Value.IndexOf("192.168.15.4", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                newApi = "http://192.168.15.4/Service";
            }
            else if (update_attr.Value.IndexOf("192.168.15.5", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                newApi = "http://192.168.15.5/Service";
            }

            var newApiComplete = newApi + "/ApiManifest/Get";

            var api_attr = node.Attributes[API_MANIFEST_ATTRIBUTE];
            if (api_attr != null && api_attr.Value.Equals(newApiComplete, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            if (!fix)
            {
                return false;
            }
            // fix
            if (api_attr == null)
            {
                api_attr = doc.CreateAttribute(API_MANIFEST_ATTRIBUTE);
                node.Attributes.InsertBefore(api_attr, node.Attributes[0]);
            }
            api_attr.Value = newApiComplete;
            doc.Save(brandingXmlFile);

            return true;
        }

    }
}
