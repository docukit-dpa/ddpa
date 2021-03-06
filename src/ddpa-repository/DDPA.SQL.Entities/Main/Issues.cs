﻿using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class Issues : ExtendedEntity<int>
    {
        public int DocId { get; set; }

        public string DepartmentId { get; set; }

        public string Issue { get; set; }

        public string SeverityLevel { get; set; }

        public string Description { get; set; }

        public DateTime? Date { get; set; }

        public string AssignedTo { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }

        public virtual Document Document { get; set; }
    }
}
