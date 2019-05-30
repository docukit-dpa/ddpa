using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Models
{
    public class UpdateDepartmentViewModel
    {
        public int Id{ get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
