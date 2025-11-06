using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDSM.AuthenticationApi.Models
{
    public class User:IdentityUser<Guid>
    {
        //public Guid Id { get { return base.Id; }; private set { Id = base.Id; } }
        //public string Name { get; set; }
        //public string Provider { get; set; }
    }
}
