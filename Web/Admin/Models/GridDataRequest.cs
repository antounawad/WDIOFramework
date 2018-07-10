using System;
using System.Collections.Generic;
using System.Linq;

namespace Admin.Models
{
    public class GridDataRequest
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; }

        public int PageSize
        {
            get => Limit;
            set => Limit = value;
        }

        public string Order { get; set; }
        public string Search { get; set; }
        public Dictionary<string, string> Filters { get; set; } = new Dictionary<string, string>();

        public void ApplyPaging<T>(ref IQueryable<T> query) => query = Limit > 0 ? query.Skip((Page - 1) * Limit).Take(Limit) : query;
    }

    public class GridDataRequest<T> : GridDataRequest
    {
        public T Custom { get; set; }
    }
}
