using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class DocumentDTO
    {
        public int Id { get; set; }

        public string CreatedDate { get; set; }

        public string DataNumber { get; set; }

        public DateTime? DueDate { get; set; }

        public Status Status { get; set; }

        public State State { get; set; }

        public RequestType RequestType { get; set; }

        public int DatasetId { get; set; }

        public string DepartmentId { get; set; }

        public List<DocumentFieldDTO> DocumentField { get; set; }

        public List<DocumentDatasetFieldDTO> DocumentDatasetField { get; set; }

    }
}
