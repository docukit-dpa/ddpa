using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class Document : ExtendedEntity<int>
    {
        public string DataNumber { get; set; }

        public DateTime? DueDate { get; set; }

        public Status Status { get; set; }

        public string DataSubject { get; set; }

        public int DatasetId { get; set; }

        public State State { get; set; }

        public RequestType RequestType { get; set; }

        public string DepartmentId { get; set; }

        public virtual ICollection<DocumentField> DocumentField { get; set; }

        public virtual ICollection<DocumentDatasetField> DocumentDatasetField { get; set; }

        public virtual ICollection<Issues> Issue { get; set; }
    }
}
