using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDMS.AuthenticationApi.Models
{
    public class Role:IdentityRole<Guid>
    {
        //public Guid Id { get; set; }
        //public string Name { get; set; }
    }
}
