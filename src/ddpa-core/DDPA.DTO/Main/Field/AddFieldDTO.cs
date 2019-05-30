using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class AddFieldDTO
    {
        public string Name { get; set; }

        public string Purpose { get; set; }

        public FieldType Type { get; set; }

        public bool IsDefault { get; set; }

        public bool IsRequired { get; set; }

        public bool IsLifeCycle { get; set; }

        public List<FieldItemDTO> FieldItem { get; set; }

        public Status LifeCycle { get; set; }

        public Classification Classification { get; set; }
    }
}
