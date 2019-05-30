using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class Department : ExtendedEntity<int>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Status { get; set; }
    }
}
