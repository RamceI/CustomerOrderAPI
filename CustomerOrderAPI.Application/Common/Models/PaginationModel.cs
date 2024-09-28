using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerOrderAPI.Application.Common.Models
{
    public class PaginationModel<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public IEnumerable<T> Results { get; set; }

        public PaginationModel(int total, int page, int size, IEnumerable<T> results)
        {
            Total = total;
            Page = page;
            Size = size;
            Results = results;
        }
    }
}
