using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class Resource : ExtendedEntity<int>
    {
        public string NameOfDocument { get; set; }

        public string TypeOfDocument { get; set; }

        public string FilePath { get; set; }
    }
}
