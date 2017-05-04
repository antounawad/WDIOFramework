using System.Xml.Serialization;

namespace Eulg.Shared
{
    [XmlRoot]
    public class BrandingInfo
    {
        [XmlAttribute]
        public bool Authenticated { get; set; }

        [XmlAttribute]
        public string Version { get; set; }

        [XmlElement]
        public string Message { get; set; }

        [XmlElement]
        public Branding Branding { get; set; }

        [XmlElement]
        public Profile Profile { get; set; }
    }
}
