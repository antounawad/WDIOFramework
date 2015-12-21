using System.Collections.Generic;

namespace Tools.KkzbGrabber
{
    interface IFormatter
    {
        void Write(IEnumerable<KeyValuePair<string, Rate>> data);
    }
}
