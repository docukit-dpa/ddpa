using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class AddModuleDTO
    {
        public string Name { get; set; }

        public string Display { get; set; }

        public string Description { get; set; }

        public bool IsEnabled { get; set; }

        public string Url { get; set; }

        public int Order { get; set; }

        public string Roles { get; set; }

        public bool View { get; set; }

        public bool Add { get; set; }

        public bool Edit { get; set; }

        public bool Delete { get; set; }

        public List<SubModuleDTO> SubModule { get; set; }
    }
}
