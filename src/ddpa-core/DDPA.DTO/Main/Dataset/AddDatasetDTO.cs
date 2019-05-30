using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.DTO
{
    public class AddDatasetDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<DatasetFieldDTO> DatasetField { get; set; }
    }
}
