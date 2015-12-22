using System;
using System.ComponentModel;
using System.IO;

namespace Tools.KkzbGrabber.Filters
{
    [Description("EndsWith(Betriebskrankenkasse) -> StartsWith(BKK)")]
    class BkkFilter : IFilter
    {
        private const string Betriebskrankenkasse = "betriebskrankenkasse";

        private int _counter;

        public bool Filter(ref Provider item)
        {
            if (item.Name.EndsWith(Betriebskrankenkasse, StringComparison.OrdinalIgnoreCase))
            {
                item = new Provider("BKK " + item.Name.Substring(item.Name.Length - Betriebskrankenkasse.Length).Trim(), item.Rate);
                ++_counter;
            }

            return true;
        }

        public void ShowAndResetCounters(TextWriter output)
        {
            output.WriteLine("{0} replacements made", _counter);
            _counter = 0;
        }
    }
}
