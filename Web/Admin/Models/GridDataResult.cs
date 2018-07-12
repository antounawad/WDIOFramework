using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Admin.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class GridDataResult
    {
        [JsonProperty(Order = 0)] public long Count { get; set; }

        protected abstract IEnumerable GetRows();
        [JsonProperty(Order = 1)] public IEnumerable Rows => GetRows();
        [JsonProperty(Order = 2, NullValueHandling = NullValueHandling.Ignore)] public object Extra { get; set; }

        public static GridDataResult<T> Create<T>(IEnumerable<T> rows) => new GridDataResult<T> { Rows = rows };
        public static GridDataResult<T> Create<T>(IEnumerable<T> rows, int count) => new GridDataResult<T> { Rows = rows, Count = count };
        public static GridDataResult<T> Create<T>(IEnumerable<T> rows, int count, IEnumerable<T> aggregates) => new GridDataResult<T> { Rows = rows, Count = count, Extra = aggregates };
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GridDataResult<T> : GridDataResult
    {
        [JsonProperty(Order = 1)]
        public new IEnumerable<T> Rows { get; set; }

        public GridDataResult() { }
        public GridDataResult(IEnumerable<T> rows, long? count = null)
        {
            Rows = rows;
            Count = count ?? Rows.Count();
        }

        protected override IEnumerable GetRows() => Rows;
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class GridDataResult<TRows, TExtra> : GridDataResult<TRows>
    {
        [JsonProperty(Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        public new TExtra Extra { get; set; }
    }
}
