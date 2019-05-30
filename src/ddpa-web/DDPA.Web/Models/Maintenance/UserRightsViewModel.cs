using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDPA.Web.Models
{
    public class UserRightsViewModel
    {
        public string Id { get; set; }

        public string ModuleId { get; set; }

        public string ModuleName { get; set; }

        public bool UserId { get; set; }

        public int View { get; set; }

        public int Add { get; set; }

        public int Edit { get; set; }

        public int Delete { get; set; }
    }
}
