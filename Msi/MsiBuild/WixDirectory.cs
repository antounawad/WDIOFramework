using System.Collections.Generic;

namespace MsiBuild
{
    public class WixDirectory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SourceName { get; set; }
        public List<WixFile> Files { get; } = new List<WixFile>();
        public List<WixDirectory> Dirs { get; } = new List<WixDirectory>();
    }
}
