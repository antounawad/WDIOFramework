﻿using System.Xml.Serialization;

namespace Eulg.Shared
{
    [XmlRoot]
    public class SetupConfig
    {
        [XmlAttribute]
        public string ApiManifestUri { get; set; }

        [XmlAttribute]
        public Branding.EUpdateChannel Channel { get; set; }
    }
}
