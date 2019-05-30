using System;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Models
{
    public class FieldItemViewModel
    {
        public string Id { get; set; }

        public int FieldId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}