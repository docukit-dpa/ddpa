using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Models
{
    public class UpdateFieldViewModel
    {
        public int Id{ get; set; }

        public string Name { get; set; }

        public string Purpose { get; set; }

        public FieldType Type { get; set; }

        public bool IsDefault { get; set; }

        public bool IsRequired { get; set; }

        public Status LifeCycle { get; set; }

        public Classification Classification { get; set; }
    }
}
