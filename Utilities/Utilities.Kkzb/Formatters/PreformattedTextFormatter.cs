using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace xbAV.Utilities.Kkzb.Formatters
{
    [Description("Preformatted text")]
    public class PreformattedTextFormatter : IFormatter
    {
        public void Write(IEnumerable<Provider> data, TextWriter buffer)
        {
            var items = data.ToList();
            var maxLength = items.Select(i => i.Name.Length).Concat(new[] { 0 }).Max();

            foreach (var item in items.OrderBy(i => i.Name))
            {
                buffer.WriteLine((item.IsNew ? "*" : "").PadRight(4) + item.Name.PadRight(maxLength) + (item.Rate * 100m).ToString("f2").PadLeft(10));
            }
        }
    }
}
