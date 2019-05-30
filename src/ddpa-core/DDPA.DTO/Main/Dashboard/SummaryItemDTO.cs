using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class SummaryItemDTO
    {
        public string Label { get; set; }

        public string Count { get; set; }

        public string Low { get; set; }

        public string Mid { get; set; }

        public string High { get; set; }

        public string Percentage { get; set; }
    }
}
