using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDSM.AuthenticationApi.Models.Request
{
    public class ExternalAuthData
    {
        public string Scheme { get; set; }
        public string ReturnUrl { get; set; }
    }
}
