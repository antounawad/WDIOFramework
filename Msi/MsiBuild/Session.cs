using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MsiBuild
{
    public class Session
    {
        public HashSet<string> ComponentIds { get; } = new HashSet<string>();

        public static Session CurrentSession = new Session();

        public void HarvestPath(string sourcePath)
        {
            var directoryInfo = new DirectoryInfo(sourcePath);
            var rootWixDir = new WixDirectory
            {
                Id = GetUniqueId(directoryInfo.Name + "_Dir"),
                Name = directoryInfo.Name
            };
            var dirs = directoryInfo.EnumerateDirectories("*.*", SearchOption.AllDirectories);
            foreach (var dir in dirs)
            {
                var wixDir = new WixDirectory
                {
                    Id = GetUniqueId(dir.Name + "_Dir"),
                    Name = dir.Name
                };
                var files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                if (files.Any())
                {
                    foreach (var file in files)
                    {

                    }
                }
                else
                {
                    // ?
                }
            }
        }

        public string GetUniqueId(string name)
        {
            var regEx = new Regex(@"^\d");
            if (regEx.IsMatch(name))
            {
                name = "_" + name;
            }
            var x = name;
            var n = 1;
            while (ComponentIds.Contains(x))
            {
                x = name + "_" + n;
                n++;
            }
            return x;
        }

    }
}
