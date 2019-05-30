using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDPA.Web.Models
{
    public class IssuesViewModel
    {
        public string Id { get; set; }

        public int DocId { get; set; }

        public string DataNumber { get; set; }

        public string DatasetName { get; set; }

        public string Department { get; set; }

        public string DepartmentId { get; set; }

        public string Issue { get; set; }

        public string SeverityLevel { get; set; }

        public string Description { get; set; }

        public DateTime? Date { get; set; }

        public string AssignedTo { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }

        
    }
}
