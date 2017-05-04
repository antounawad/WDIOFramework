using System.IO;

namespace xbAV.Utilities.Kkzb
{
    public interface IFilter
    {
        bool Initialize();
        bool Filter(ref Provider item);
        void ShowAndResetCounters(TextWriter output);
    }
}
