using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class AddSubModuleFieldDTO
    {
        public int SubModuleId { get; set; }

        public int FieldId { get; set; }

        public int Order { get; set; }

        public FieldDTO Field { get; set; }
    }
}
