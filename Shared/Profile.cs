using System.Xml.Serialization;

namespace Eulg.Shared
{
    [XmlRoot]
    public class Profile
    {
        [XmlAttribute]
        public string StartMenuFolder { get; set; }

        [XmlAttribute]
        public string DesktopApp { get; set; }

        [XmlAttribute]
        public string DesktopWeb { get; set; }

        [XmlAttribute]
        public string StartMenuApp { get; set; }

        [XmlAttribute]
        public string StartMenuSync { get; set; }

        [XmlAttribute]
        public string StartMenuSupportTool { get; set; }

        [XmlAttribute]
        public string StartMenuFernwartung { get; set; }
    }
}
