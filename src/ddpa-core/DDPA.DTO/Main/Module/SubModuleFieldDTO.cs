using System;
using System.Collections.Generic;
using System.Text;

namespace DDPA.DTO
{
    public class SubModuleFieldDTO
    {
        public int SubModuleId { get; set; }

        public int FieldId { get; set; }

        public int Order { get; set; }

        public FieldDTO Field { get; set; }
    }
}
