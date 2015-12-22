using System.Collections.Generic;

namespace Tools.KkzbGrabber
{
    interface IGrabber
    {
        IEnumerable<Provider> GetBeitraege();
    }
}
