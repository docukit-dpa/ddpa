using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class AddDocumentDTO
    {
        public string DataNumber { get; set; }

        public DateTime? DueDate { get; set; }

        public Status Status { get; set; }

        public int DatasetId { get; set; }

        public int DepartmentId { get; set; }

        public List<DocumentFieldDTO> DocumentField { get; set; }

        public List<DocumentDatasetFieldDTO> DocumentDatasetField { get; set; }

        public ButtonAction ButtonAction { get; set; }
    }
}
