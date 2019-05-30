using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.Web.Models
{
    public class UpdateUserViewModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string Role { get; set; }

        public string DepartmentId { get; set; }

        public TypeOfNotification TypeOfNotification { set; get; }

        public string Permissions { get; set; }

    }
}
