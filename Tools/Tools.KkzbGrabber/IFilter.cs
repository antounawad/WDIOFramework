using System.Collections.Generic;

namespace Tools.KkzbGrabber
{
    interface IFilter
    {
        bool Filter(ref KeyValuePair<string, Rate> item);
    }
}
