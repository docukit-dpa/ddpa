using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDPA.Web.Models
{
    public class ModuleViewModel
    {
        public string Name { get; set; }

        public string Display { get; set; }

        public string Description { get; set; }

        public bool IsEnabled { get; set; }

        public string Url { get; set; }

        public int Order { get; set; }

        public bool View { get; set; }

        public bool Add { get; set; }

        public bool Edit { get; set; }

        public List<SubModuleViewModel> SubModule { get; set; }
    }
}
