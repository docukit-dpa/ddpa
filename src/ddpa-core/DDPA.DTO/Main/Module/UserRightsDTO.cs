using System;
using System.Collections.Generic;
using System.Text;

namespace DDPA.DTO
{
    public class UserRightsDTO
    {
        public string Id { get; set; }

        public string ModuleId { get; set; }

        public string UserId { get; set; }

        public bool View { get; set; }

        public bool Add { get; set; }

        public bool Edit { get; set; }

        public bool Delete { get; set; }
    }
}
