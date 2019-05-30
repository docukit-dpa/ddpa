using System;
using System.Collections.Generic;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Models
{
    public class LogsViewModel
    {
        public string DocId { get; set; }

        public string DataNumber { get; set; }

        public string DatasetName { get; set; }

        public string UserId { get; set; }

        public string Action { get; set; }

        public string Description { get; set; }

        public string Comment { get; set; }

        public string ActionDate { get; set; }

    }
}