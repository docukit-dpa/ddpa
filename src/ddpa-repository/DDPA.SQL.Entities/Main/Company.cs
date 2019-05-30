using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class Company : BaseEntity<int>
    {
        public string Name { get; set; }

        public string ContactNo { get; set; }

        public string Email { get; set; }

        public string Description { get; set; }
    }
}
