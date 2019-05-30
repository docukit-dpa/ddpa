using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using static DDPA.Commons.Enums.DDPAEnums;

namespace DDPA.SQL.Entities
{
    public class ExtendedIdentityUser : IdentityUser<int>, IEntity
    {
        public string FirstName { set; get; }

        public string LastName { set; get; }

        public string DepartmentId { set; get; }

        public string CreatedBy { set; get; }

        public TypeOfNotification TypeOfNotification { set; get; }

        public bool HasPasswordChanged { set; get; }

        public int DoneSetUp { set; get; }
    }
}
