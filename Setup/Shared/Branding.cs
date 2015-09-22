using System.IO;
using System.Xml.Serialization;

namespace Eulg.Setup.Shared
{
    [XmlRoot]
    public class Branding
    {
        [XmlIgnore] public const int BRANDING_VERSION = 1;

        public enum EUpdateChannel
        {
            Release = 0,
            Beta = 1,
            Demo = 2,
            Proof = 3
        }

        [XmlElement]
        public InfoConfig Info { get; set; }

        public class InfoConfig
        {
            [XmlAttribute]
            public string BuildTag { get; set; }

            [XmlAttribute]
            public string BuildDescription { get; set; }
        }

        [XmlElement]
        public FileSystemConfig FileSystem { get; set; }

        public class FileSystemConfig
        {
            [XmlAttribute]
            public string AppDir { get; set; }

            [XmlAttribute]
            public string DataDir { get; set; }

            [XmlAttribute]
            public string AppBinary { get; set; }

            [XmlAttribute]
            public string SyncBinary { get; set; }
        }

        [XmlElement]
        public RegistryConfig Registry { get; set; }

        public class RegistryConfig
        {
            [XmlAttribute]
            public string UserSettingsKey { get; set; }

            [XmlAttribute]
            public string MachineSettingsKey { get; set; }

            [XmlAttribute]
            public string UninstallKey { get; set; }

            [XmlAttribute]
            public string UninstallName { get; set; }

            [XmlAttribute]
            public string LocalDbInstance { get; set; }
        }

        [XmlElement]
        public ShellIconsConfig ShellIcons { get; set; }

        public class ShellIconsConfig
        {
            [XmlAttribute]
            public string DesktopApp { get; set; }

            [XmlAttribute]
            public string StartMenuFolder { get; set; }

            [XmlAttribute]
            public string StartMenuApp { get; set; }

            [XmlAttribute]
            public string StartMenuSync { get; set; }

            [XmlAttribute]
            public string StartMenuSupportTool { get; set; }

            [XmlAttribute]
            public string StartMenuFernwartung { get; set; }
        }

        [XmlElement]
        public UpdateConfig Update { get; set; }

        public class UpdateConfig
        {
            [XmlAttribute]
            public EUpdateChannel Channel { get; set; }

            [XmlAttribute]
            public bool UseHttps { get; set; }
        }

        [XmlElement]
        public UrlsConfig Urls { get; set; }

        public class UrlsConfig
        {
            [XmlAttribute]
            public string Update { get; set; }

            [XmlAttribute]
            public string Web { get; set; }

            [XmlAttribute]
            public string Sync { get; set; }

            [XmlAttribute]
            public string SyncHttp { get; set; }

            [XmlAttribute]
            public string DbUpdateUrl { get; set; }

            [XmlAttribute]
            public string CertPublicKey { get; set; }
        }

        public static Branding Read(string filename)
        {
            using (var fileStream = File.OpenRead(filename))
            {
                var xmlSerializer = new XmlSerializer(typeof(Branding));
                return xmlSerializer.Deserialize(fileStream) as Branding;
            }
        }

        public void Write(string filename)
        {
            using (var fileStream = File.Open(filename, FileMode.Create))
            {
                var xmlSerializer = new XmlSerializer(typeof(Branding));
                xmlSerializer.Serialize(fileStream, this);
            }
        }

        public static Branding Current { get; set; }
    }
}
