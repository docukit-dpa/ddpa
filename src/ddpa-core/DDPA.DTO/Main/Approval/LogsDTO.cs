using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class LogsDTO
    {
        public string Id { get; set; }

        public string DocId { get; set; }

        public string DataNumber { get; set; }

        public string DatasetName { get; set; }

        public string UserId { get; set; }

        public string Action { get; set; }

        public string Description { get; set; }

        public string Comment { get; set; }

        public DateTime? ActionDate { get; set; }
    }
}
