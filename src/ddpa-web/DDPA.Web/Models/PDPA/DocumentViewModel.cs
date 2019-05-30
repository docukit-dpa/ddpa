using System;
using System.Collections.Generic;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Models
{
    public class DocumentViewModel
    {
        public int Id { get; set; }

        public string DataNumber { get; set; }

        public DateTime? DueDate { get; set; }

        public Status Status { get; set; }

        public State State { get; set; }

        public RequestType RequestType { get; set; }

        public int DatasetId { get; set; }

        public string DepartmentId { get; set; }

        public List<DocumentFieldViewModel> DocumentField { get; set; }

        public List<DocumentDatasetFieldViewModel> DocumentDatasetField { get; set; }

        public string JsonDocumentField { get; set; }

        public List<FieldViewModel> Field { get; set; }

        public List<FieldDatasetViewModel> FieldDataset { get; set; }

        public string SubModuleId { get; set; }

        public string DatasetName { get; set; }

        public List<DatasetViewModel> Datasets { get; set; }

        public string JsonDatasetField { get; set; }
        
        public ButtonAction ButtonAction { get; set; }
    }
}