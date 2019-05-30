using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDPA.Web.Models
{
    public class AddResourceViewModel
    {
        public string TypeOfDocument { get; set; }

        public string NameOfDocument { get; set; }

        public IFormFile FilePath { get; set; }
    }
}
