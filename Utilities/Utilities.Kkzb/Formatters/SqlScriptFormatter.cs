using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace xbAV.Utilities.Kkzb.Formatters
{
    [Description("T-SQL update script")]
    public class SqlScriptFormatter : IFormatter
    {
        public void Write(IEnumerable<Provider> data, TextWriter buffer)
        {
            var items = data.ToList();

            var availabilityDate = GetAvailabilityDate();
            var availabilityDateFormat = availabilityDate.ToString("yyyy-MM-dd");

            var newProviders = items.Where(i => i.IsNew).OrderBy(i => i.Name).ToList();
            if (newProviders.Any())
            {
                var statement = "INSERT INTO dbo.HealthInsuranceProvider VALUES\r\n\t" + string.Join(",\r\n\t", newProviders.Select(p =>
                {
                    var escaped = EscapeSingleQuote(p.Name);
                    return $"\t(NEWID(), '{escaped}', '{escaped}')";
                })) + ";";

                buffer.WriteLine(statement);
                buffer.WriteLine();
            }

            foreach(var item in items.OrderBy(i => i.Name))
            {
                var rate = ((decimal)item.Rate).ToString("f4", CultureInfo.InvariantCulture);
                buffer.WriteLine($"INSERT INTO dbo.HealthInsuranceSurcharge VALUES ((SELECT [ID] FROM dbo.HealthInsuranceProvider WHERE [Name]='{EscapeSingleQuote(item.Name)}'), {rate}, '{availabilityDateFormat}T00:00:01.000Z');");
            }
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
