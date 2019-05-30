using System;
using System.Collections.Generic;
using System.Text;

namespace DDPA.DTO
{
    public class ChangePasswordUserDTO
    {
        public string Id { get; set; }

        public string OldPassword { get; set; }

        public string ConfirmPassword { get; set; }

        public string NewPassword { get; set; }

        public string Email { get; set; }
    }
}
