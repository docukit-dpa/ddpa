using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class Module : ExtendedEntity<int>
    {
        public string Name { get; set; }

        public string Display { get; set; }

        public string Description { get; set; }

        public bool isEnabled { get; set; }

        public string Url { get; set; }

        public int Order { get; set; }

        public string Roles { get; set; }

        public bool View { get; set; }

        public bool Add { get; set; }

        public bool Edit { get; set; }

        public bool Delete { get; set; }

        public virtual ICollection<SubModule> SubModule { get; set; }
    }
}
