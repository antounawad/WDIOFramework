using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MsiBuild
{
    public class Harvester
    {
        



        public void HarvestPath(string sourcePath, string outputFile)
        {

            var sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<Include>");


            sb.AppendLine("</Include>");

            File.WriteAllText(outputFile, sb.ToString());
        }

    }
}
