using System.Collections.Generic;

namespace xbAV.Utilities.Kkzb
{
    public interface IGrabber
    {
        IEnumerable<Provider> GetBeitraege();
    }
}
