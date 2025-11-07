using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDMS.AuthenticationApi.Models.Request
{
    public class Register:BaseAuthentication
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
