using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDPA.Web.Models
{
    public class SendMailViewModel
    {
        public string name { get; set; }

        public string organization { get; set; }

        public string email { get; set; }

        public string message { get; set; }
    }
}
