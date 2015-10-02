using System;
using System.Collections.Generic;

namespace Eulg.Update.Common
{
    public class UpdateItemEquality : IEqualityComparer<IUpdateItem>
    {
        private static readonly UpdateItemEquality Inst = new UpdateItemEquality();

        private UpdateItemEquality() { }

        public static IEqualityComparer<IUpdateItem> Instance { get { return Inst; } }

        public bool Equals(IUpdateItem x, IUpdateItem y)
        {
            return x.FilePath.Equals(y.FilePath, StringComparison.InvariantCultureIgnoreCase)
                   && x.FileName.Equals(y.FileName, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(IUpdateItem obj)
        {
            unchecked
            {
                return obj.FilePath.ToLowerInvariant().GetHashCode() * 17
                       + obj.FileName.ToLowerInvariant().GetHashCode();
            }
        }
    }
}
