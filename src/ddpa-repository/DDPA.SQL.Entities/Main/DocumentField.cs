using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DDPA.SQL.Entities
{
    public class DocumentField : BaseEntity<int>
    {
        public int DocumentId { get; set; }

        public int SubModuleId { get; set; }

        public int FieldId { get; set; }

        public string Value { get; set; }

        public string NewValue { get; set; }

        public bool IsEdited { get; set; }

        public string FilePath { get; set; }

        public virtual Document Document { get; set; }

        public virtual Field Field { get; set; }
    }
}
