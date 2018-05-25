using System.Collections.Generic;
using System.IO;

namespace xbAV.Utilities.Kkzb
{
    public interface IFormatter
    {
        void Write(IEnumerable<Provider> data, TextWriter buffer);
    }
}
