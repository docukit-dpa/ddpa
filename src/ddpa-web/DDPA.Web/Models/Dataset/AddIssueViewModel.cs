using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDPA.Web.Models
{
    public class AddIssueViewModel
    {
        public string DocId { get; set; }

        public string Issue { get; set; }

        public string SeverityLevel { get; set; }

        public string Description { get; set; }

        public DateTime? Date { get; set; }

        public string AssignedTo { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }
    }
}
