using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class AddUserDTO
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string Role { get; set; }

        public string DepartmentId { get; set; }

        public string CreatedBy { get; set; }

        public bool EmailConfirmed { get; set; }

        public TypeOfNotification TypeOfNotification { set; get; }
    }
}
