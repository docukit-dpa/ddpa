using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Models
{
    public class DocumentFieldViewModel
    {
        public int Id { get; set; }

        public int SubModuleId { get; set; }

        public int FieldId { get; set; }

        public string Value { get; set; }

        public string NewValue { get; set; }

        public bool IsEdited { get; set; }

        public FieldViewModel Field { get; set; }

        public string FilePath { get; set; }

        public bool File { get; set; }
    }
}