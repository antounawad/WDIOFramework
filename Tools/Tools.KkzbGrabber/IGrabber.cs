using System.Collections.Generic;

namespace Tools.KkzbGrabber
{
    interface IGrabber
    {
        IEnumerable<KeyValuePair<string, Rate>> GetBeitraege();
    }
}
