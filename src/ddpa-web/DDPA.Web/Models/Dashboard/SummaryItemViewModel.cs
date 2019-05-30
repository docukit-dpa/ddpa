using System;
using DDPA.DTO;

namespace DDPA.Web.Models
{
    public class SummaryItemViewModel
    {
        public string Label { get; set; }

        public string Count { get; set; }

        public string Low { get; set; }

        public string Mid { get; set; }

        public string High { get; set; }

        public string Percentage { get; set; }
    }
}