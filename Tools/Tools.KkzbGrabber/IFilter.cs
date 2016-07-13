using System.IO;

namespace Tools.KkzbGrabber
{
    interface IFilter
    {
        bool Filter(ref Provider item);
        void ShowAndResetCounters(TextWriter output);
    }
}
