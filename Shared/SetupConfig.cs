using System.Xml.Serialization;

namespace Eulg.Shared
{
    [XmlRoot]
    public class SetupConfig
    {
        [XmlElement]
        public string Version { get; set; }

        [XmlElement]
        public Branding Branding { get; set; }
    }
}
