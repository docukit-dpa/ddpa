using System;
using System.Collections.Generic;
using System.Text;

namespace DDPA.SQL.Entities
{
    public class Dataset : ExtendedEntity<int>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public virtual ICollection<DatasetField> DatasetField { get; set; }
    }
}
