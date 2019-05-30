using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class FieldDTO
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public FieldType Type { get; set; }

        public String LifeCycle { get; set; }

        public bool IsDefault { get; set; }

        public bool IsRequired { get; set; }

        public List<FieldItemDTO> FieldItem { get; set; }

        public string Value { get; set; }

        public string TypeName { get; set; }

        public string Purpose { get; set; }

        public Classification Classification { get; set; }
    }
}
