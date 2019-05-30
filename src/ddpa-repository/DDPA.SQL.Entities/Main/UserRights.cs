using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class UserRights : BaseEntity<int>
    {

        public int ModuleId { get; set; }

        public int UserId { get; set; }

        public bool View { get; set; }

        public bool Add { get; set; }

        public bool Edit { get; set; }

        public bool Delete { get; set; }

    }
}
