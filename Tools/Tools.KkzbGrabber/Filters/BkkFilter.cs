using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Tools.KkzbGrabber.Filters
{
    [Description("EndsWith(Betriebskrankenkasse) -> StartsWith(BKK)")]
    class BkkFilter : IFilter
    {
        private const string Betriebskrankenkasse = "betriebskrankenkasse";

        public bool Filter(ref KeyValuePair<string, Rate> item)
        {
            if (item.Key.EndsWith(Betriebskrankenkasse, StringComparison.OrdinalIgnoreCase))
            {
                item = new KeyValuePair<string, Rate>("BKK " + item.Key.Substring(item.Key.Length - Betriebskrankenkasse.Length).Trim(), item.Value);
            }

            return true;
        }
    }
}
