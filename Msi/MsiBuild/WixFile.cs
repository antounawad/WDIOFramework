using System;
using System.Text;

namespace MsiBuild
{
    public class WixFile
    {
        public string Id { get; set; }
        public Guid? Guid { get; set; }
        public bool? KeyPath { get; set; }

        public string FileId { get; set; }
        public bool? FileKeyPath { get; set; }
        public string FileSource { get; set; }
        public string FileName { get; set; }
        public string FileShortName { get; set; }
        public bool? FileVital { get; set; }
        public string FileDiskId { get; set; }

        public string GetString(int indentation = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine(new string(' ', indentation) + $"<Component Id=\"{Id}\" Guid=\"{(Guid.HasValue ? Guid.ToString() : "*")}\"{(KeyPath.HasValue ? $"KeyPath=\"{(KeyPath.Value ? "yes" : "no")}\"" : string.Empty)}>");
            indentation += 4;
            sb.Append(new string(' ', indentation) + $"<File Id=\"{FileId}\" Source=\"{FileSource}\"");

            if (FileKeyPath.HasValue) sb.Append($" KeyPath=\"{(FileKeyPath.Value ? "yes" : "no")}\"");
            if (FileName != null) sb.Append($" Name=\"{FileName}\"");
            if (FileShortName != null) sb.Append($" ShortName=\"{FileShortName}\"");
            if (FileVital.HasValue) sb.Append($" Vital=\"{(FileVital.Value ? "yes" : "no")}\"");
            if (FileDiskId != null) sb.Append($" DiskId=\"{FileDiskId}\"");

            sb.Append(" />" + Environment.NewLine);
            indentation -= 4;
            sb.AppendLine(new string(' ', indentation) + "</Component>");
            return sb.ToString();
        }
    }
}
