using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class WorkflowInbox : ExtendedEntity<int>
    {
        public string ApproverRole { get; set; }

        public string CreatedBy { get; set; }

        public string DepartmentId { get; set; }

        public string DocumentId { get; set; }

        public Status Status { get; set; }

    }
}
