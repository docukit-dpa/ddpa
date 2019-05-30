using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDPA.Web.Models
{
    public class FieldDatasetViewModel
    {
        public int Id { get; set; }

        public int DatasetId { get; set; }

        public int FieldId { get; set; }

        public FieldViewModel Field { get; set; }

        public string Value { get; set; }
    }
}
