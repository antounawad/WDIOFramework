using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Eulg.Update.Shared
{
    public class WorkerConfig
    {
        [XmlElement]
        public string AppPath { get; set; }

        [XmlElement]
        public string TempPath { get; set; }

        [XmlElement]
        public string ApplicationFile { get; set; }

        [XmlElement]
        public string CommandLineArgs { get; set; }

        [XmlElement]
        public int WaitForProcess { get; set; }

        [XmlElement]
        public WorkerProcess StartProcess { get; set; }

        [XmlElement]
        public string CheckProcesses { get; set; }

        [XmlElement]
        public string KillProcesses { get; set; }

        [XmlElement]
        public string LogFile { get; set; }

        public class WorkerFile
        {
            [XmlAttribute]
            public string Source { get; set; }

            [XmlAttribute]
            public string Destination { get; set; }

            [XmlAttribute]
            public DateTime FileDateTime { get; set; }

            [XmlIgnore]
            public long FileSize { get; set; }

            [XmlIgnore]
            public long FileSizeGz { get; set; }

            [XmlIgnore]
            public string FileName { get; set; }
            [XmlIgnore]
            public bool NewFile { get; set; }
            [XmlIgnore]
            public bool Done { get; set; }
            [XmlIgnore]
            public string Checksum { get; set; }
        }

        public class WorkerDelete
        {
            [XmlAttribute]
            public string Path { get; set; }
        }

        public readonly List<WorkerFile> WorkerFiles = new List<WorkerFile>();
        public readonly List<WorkerDelete> WorkerDeletes = new List<WorkerDelete>();
    }
}
