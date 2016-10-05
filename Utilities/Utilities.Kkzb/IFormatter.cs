using System.Collections.Generic;

namespace xbAV.Utilities.Kkzb
{
    public interface IFormatter
    {
        void Write(IEnumerable<Provider> data);
    }
}
