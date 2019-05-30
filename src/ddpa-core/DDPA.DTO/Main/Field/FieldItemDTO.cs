using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class FieldItemDTO
    {
        public int Id { get; set; }

        public int FieldId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
