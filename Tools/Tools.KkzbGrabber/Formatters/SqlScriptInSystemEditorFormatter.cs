using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Tools.KkzbGrabber.Formatters
{
    [Description("Show T-SQL update script in default editor")]
    class SqlScriptInSystemEditorFormatter : IFormatter
    {
        public void Write(IEnumerable<Provider> data)
        {
            var items = data.ToList();

            var availabilityDate = GetAvailabilityDate();
            var availabilityDateFormat = availabilityDate.ToString("yyyy-MM-dd");

            var filename = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), ".txt"));
            using(var file = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var newProviders = items.Where(i => i.IsNew).OrderBy(i => i.Name).ToList();
                if (newProviders.Any())
                {
                    var statement = "INSERT INTO dbo.HealthInsuranceProvider VALUES\r\n\t" + string.Join(",\r\n\t", newProviders.Select(p =>
                    {
                        var escaped = EscapeSingleQuote(p.Name);
                        return $"\t(NEWID(), '{escaped}', '{escaped}')";
                    })) + ";\r\n\r\n";

                    var bytes = Encoding.UTF8.GetBytes(statement);
                    file.Write(bytes, 0, bytes.Length);
                }

                foreach(var item in items.OrderBy(i => i.Name))
                {
                    var line = $"INSERT INTO dbo.HealthInsuranceSurcharge VALUES ((SELECT [ID] FROM dbo.HealthInsuranceProvider WHERE [Name]='{EscapeSingleQuote(item.Name)}'), {(decimal)item.Rate:f4}, '{availabilityDateFormat} 00:00:01.000');\r\n";
                    var bytes = Encoding.UTF8.GetBytes(line);
                    file.Write(bytes, 0, bytes.Length);
                }
            }

            using(Process.Start(filename)) { }
        }

        private static string EscapeSingleQuote(string str)
        {
            return str.Replace("'", "''");
        }

        private static DateTime GetAvailabilityDate()
        {
            //TODO prompt user, offer "today" as default
            return DateTime.Today;
        }
    }
}
