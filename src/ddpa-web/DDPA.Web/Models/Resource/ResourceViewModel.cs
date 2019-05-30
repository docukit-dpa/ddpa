using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDPA.Web.Models
{
    public class ResourceViewModel
    {
        public int Id { get; set; }

        public string TypeOfDocument { get; set; }

        public string NameOfDocument { get; set; }

        public string FilePath { get; set; }
    }
}
