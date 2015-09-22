using System.Xml.Serialization;

namespace Eulg.Setup.Shared
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
