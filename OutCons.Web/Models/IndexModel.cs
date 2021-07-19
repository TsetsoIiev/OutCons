using System;
using System.Collections.Generic;

namespace OutCons.Web.Models
{
    public class IndexModel
    {
        public List<User> Users { get; init; }

        public int PageIndex { get; set; }

        public int TotalPages { get; set; }
        
        public bool HasPreviousPage => (PageIndex > 1);

        public bool HasNextPage => (PageIndex < TotalPages);

        public DateTime DateFrom { get; set; }
        

        public DateTime DateTo { get; set; }
    }
}