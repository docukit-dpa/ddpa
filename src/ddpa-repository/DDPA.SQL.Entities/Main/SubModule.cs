using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class SubModule : BaseEntity<int>
    {
        public int ModuleId { get; set; }

        public string Name { get; set; }

        public string Display { get; set; }

        public string Description { get; set; }

        public bool isEnabled { get; set; }

        public string Url { get; set; }

        public int Order { get; set; }

        public string Roles { get; set; }

        public virtual Module Module { get; set; }

        public virtual ICollection<SubModuleField> SubModuleField { get; set; }
    }
}
