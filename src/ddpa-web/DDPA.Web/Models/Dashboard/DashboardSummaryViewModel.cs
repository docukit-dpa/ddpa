using System;
using System.Collections.Generic;
using DDPA.DTO;

namespace DDPA.Web.Models
{
    public class DashboardSummaryViewModel
    {
        public SummaryItemViewModel DataSet { get; set; }

        public SummaryItemViewModel Storage { get; set; }

        public SummaryItemViewModel ExternalParty { get; set; }

        public SummaryItemViewModel Issue { get; set; }
    }
}