using System;
using System.Collections.Generic;

namespace DDPA.Web.Models
{
    public class SearchViewModel
    {
        public int Draw { get; set; }
        // page number
        public int Start { get; set; }
        // page size or number of records per page
        public int Length { get; set; }

        public Dictionary<string, string> SearchParameters { get; set; }

        public List<Dictionary<string, string>> Order { get; set; }

        public int Status { get; set; }
    }
}