using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class SubModuleField : BaseEntity<int>
    {
        public int SubModuleId { get; set; }

        public int FieldId { get; set; }

        public int Order { get; set; }

        public virtual SubModule SubModule { get; set; }

        public virtual Field Field { get; set; }
    }
}
