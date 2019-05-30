using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class FieldItem : BaseEntity<int>
    {
        public int FieldId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual Field Field { get; set; }
    }
}
