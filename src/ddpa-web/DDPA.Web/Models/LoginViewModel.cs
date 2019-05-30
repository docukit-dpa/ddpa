using System;

namespace DDPA.Web.Models
{
    public class LoginViewModel
    {
        public string Email { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}