using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eulg.Update.Common
{
    public class UpdateConfig
    {
        [XmlElement]
        public string Version { get; set; }

        [XmlElement]
        public int BrandingVersion { get; set; }

        public class UpdateFile : IUpdateItem
        {
            [XmlAttribute]
            public string FilePath { get; set; }
            [XmlAttribute]
            public string FileName { get; set; }
            [XmlAttribute]
            public DateTime FileDateTime { get; set; }
            [XmlAttribute]
            public string CheckSum { get; set; }
            [XmlAttribute]
            public long FileSize { get; set; }
            [XmlAttribute]
            public long FileSizeGz { get; set; }
        }

        public class UpdateDelete : IUpdateItem
        {
            [XmlAttribute]
            public string FilePath { get; set; }
            [XmlAttribute]
            public string FileName { get; set; }
        }

        //public class Plugin
        //{
        //    [XmlAttribute]
        //    public string Name { get; set; }
        //}

        //public class Account
        //{
        //    [XmlAttribute]
        //    public string UserName { get; set; }
        //    [XmlAttribute]
        //    public string AgencyID { get; set; }
        //    [XmlAttribute]
        //    public string AgencyName { get; set; }
        //    [XmlAttribute]
        //    public string AgencyNameShort { get; set; }
        //    [XmlAttribute]
        //    public bool IsDemoUser { get; set; }
        //}

        public class ResetFile : IUpdateItem
        {
            [XmlAttribute]
            public string FilePath { get; set; }
            [XmlAttribute]
            public string FileName { get; set; }
        }

        public readonly List<UpdateFile> UpdateFiles = new List<UpdateFile>();
        public readonly List<UpdateDelete> UpdateDeletes = new List<UpdateDelete>();
        public readonly List<ResetFile> ResetFiles = new List<ResetFile>();

        //public readonly List<Plugin> Plugins = new List<Plugin>();
        //public readonly List<Account> Accounts = new List<Account>();
    }
}
