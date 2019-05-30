using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class SubModuleDTO
    {
        public string Id { get; set; }

        public int ModuleId { get; set; }

        public string Name { get; set; }

        public string Display { get; set; }

        public string Description { get; set; }

        public bool isEnabled { get; set; }

        public string Url { get; set; }

        public int Order { get; set; }

        public string Roles { get; set; }

        public List<SubModuleFieldDTO> SubModuleField { get; set; }
    }
}
