using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDPA.Web.Models
{
    public class DatasetTableViewModel
    {
        public string Id { get; set; }

        public string CreatedDate { get; set; }

        public string DataNumber { get; set; }

        public string DatasetName { get; set; }

        public string Department { get; set; }

        public string DataOwner { get; set; }

        public string Storage { get; set; }

        public string PurposeOfUse { get; set; }

        public string OutsideSingapore { get; set; }

        public string RetentionPeriod { get; set; }

        public string DisposalMethod { get; set; }
    }
}
