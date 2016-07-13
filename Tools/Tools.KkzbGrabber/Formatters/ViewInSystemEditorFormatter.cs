using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Tools.KkzbGrabber.Formatters
{
    [Description("View data in default text editor")]
    class ViewInSystemEditorFormatter : IFormatter
    {
        public void Write(IEnumerable<Provider> data)
        {
            var items = data.ToList();
            var maxLength = items.Select(i => i.Name.Length).Concat(new[] { 0 }).Max();

            var filename = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".txt"));
            using(var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                foreach (var item in items.OrderBy(i => i.Name))
                {
                    var line = (item.IsNew ? "*" : "").PadRight(4) + item.Name.PadRight(maxLength) + (item.Rate * 100m).ToString("f2").PadLeft(10) + Environment.NewLine;
                    var bytes = Encoding.UTF8.GetBytes(line);
                    file.Write(bytes, 0, bytes.Length);
                }
            }

            using (Process.Start(filename)) { }
        }
    }
}
