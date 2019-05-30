using System;
using System.Collections.Generic;
using System.Text;

namespace DDPA.SQL.Entities
{
    public class DatasetField : BaseEntity<int>
    {
        public int DatasetId { get; set; }

        public int FieldId { get; set; }
        
        public int Order { get; set; }

        public virtual Dataset Dataset { get; set; }

        public virtual Field Field { get; set; }
    }
}
