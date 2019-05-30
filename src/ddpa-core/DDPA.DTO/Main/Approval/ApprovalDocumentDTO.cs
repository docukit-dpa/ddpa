using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class ApprovalDocumentDTO
    {
        public string Id { get; set; }

        public string DataNumber { get; set; }

        public string DepartmentId { get; set; }

        public string DatasetName { get; set; }

        public int DatasetId { get; set; }

        public string Details { get; set; }

        public string Logs { get; set; }

        public State State { get; set; }

        public Status Status { get; set; }

        public RequestType RequestType { get; set; }

        public string createdBy { get; set; }
    }
}
