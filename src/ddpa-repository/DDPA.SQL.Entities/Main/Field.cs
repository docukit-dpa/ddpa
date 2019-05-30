using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class Field : ExtendedEntity<int>
    {
        public string Name { get; set; }

        public string Purpose { get; set; }

        public FieldType Type { get; set; }

        public bool IsDefault { get; set; }

        public bool IsRequired { get; set; }

        public bool IsLifeCycle { get; set; }

        public Classification Classification { get; set; }

        public virtual ICollection<FieldItem> FieldItem { get; set; }

        public virtual ICollection<SubModuleField> SubModuleField { get; set; }

        public virtual ICollection<DatasetField> DatasetField { get; set; }
    }
}
