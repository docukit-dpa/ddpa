using System;
using System.Collections.Generic;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Models
{
    public class ApprovalDocumentViewModel
    {
        public string Id { get; set; }

        public string DataNumber { get; set; }
        
        public string DepartmentId { get; set; }

        public string DatasetName { get; set; }

        public string DatasetId { get; set; }

        public string Details { get; set; }

        public string Logs { get; set; }

        public string State { get; set; }

        public string Status { get; set; }

        public RequestType RequestType { get; set; }

    }
}