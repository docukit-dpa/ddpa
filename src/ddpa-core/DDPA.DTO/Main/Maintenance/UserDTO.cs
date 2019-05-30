using System;
using System.Collections.Generic;
using System.Text;

namespace DDPA.DTO
{
    public class UserDTO
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }

        public string DepartmentId { get; set; }

        public bool EmailConfirmed { get; set; }
    }
}
