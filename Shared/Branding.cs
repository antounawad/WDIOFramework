using System.IO;
using System.Xml.Serialization;

namespace Eulg.Shared
{
    [XmlRoot]
    public class Branding
    {
        [XmlIgnore]
        // ReSharper disable once InconsistentNaming
        public const int BrandingVersion = 2;

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
            public virtual EUpdateChannel Channel { get; set; }

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
