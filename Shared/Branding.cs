using System;
using System.IO;
using System.Xml.Serialization;

namespace Eulg.Shared
{
    [XmlRoot]
    public class Branding
    {
        [XmlIgnore]
        public const int BrandingVersion = 1;

        public enum EUpdateChannel
        {
            Release = 0,
            Beta = 1,
            Demo = 2,
            Proof = 3,
        }

        [XmlElement]
        public virtual InfoConfig Info { get; set; }
        public class InfoConfig
        {
            [XmlAttribute]
            public virtual string BuildTag { get; set; }

            [XmlAttribute]
            public virtual string BuildDescription { get; set; }
        }

        [XmlElement]
        public virtual FileSystemConfig FileSystem { get; set; }
        public class FileSystemConfig
        {
            [XmlAttribute]
            public virtual string AppDir { get; set; }

            [XmlAttribute]
            public virtual string DataDir { get; set; }

            [XmlAttribute]
            public virtual string AppBinary { get; set; }

            [XmlAttribute]
            public virtual string SyncBinary { get; set; }
        }

        [XmlElement]
        public virtual RegistryConfig Registry { get; set; }
        public class RegistryConfig
        {
            [XmlAttribute]
            public virtual string UserSettingsKey { get; set; }

            [XmlAttribute]
            public virtual string MachineSettingsKey { get; set; }

            [XmlAttribute]
            public virtual string UninstallKey { get; set; }

            [XmlAttribute]
            public virtual string UninstallName { get; set; }
        }

        [XmlElement]
        public virtual ShellIconsConfig ShellIcons { get; set; }

        [Obsolete("Profil wird aus Theme ermittelt")]
        public class ShellIconsConfig
        {
            [XmlAttribute]
            public virtual string DesktopApp { get; set; }

            [XmlAttribute]
            public virtual string StartMenuFolder { get; set; }

            [XmlAttribute]
            public virtual string StartMenuApp { get; set; }

            [XmlAttribute]
            public virtual string StartMenuSync { get; set; }

            [XmlAttribute]
            public virtual string StartMenuSupportTool { get; set; }

            [XmlAttribute]
            public virtual string StartMenuFernwartung { get; set; }
        }

        [XmlElement]
        public virtual UpdateConfig Update { get; set; }
        public class UpdateConfig
        {
            [XmlAttribute]
            public virtual EUpdateChannel Channel { get; set; }
        }

        [XmlElement]
        public virtual UrlsConfig Urls { get; set; }
        public class UrlsConfig
        {
            [XmlAttribute]
            public virtual string Update { get; set; }

            [XmlAttribute]
            public virtual string Web { get; set; }
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
    }
}
