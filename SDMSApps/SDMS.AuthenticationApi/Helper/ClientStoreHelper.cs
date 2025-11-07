using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDMS.AuthenticationApi.Helper
{
    public class ClientStoreHelper:IClientStore
    {
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return Task.FromResult(StaticDataHelper.Clients.FirstOrDefault(x => x.ClientId.Equals(clientId, StringComparison.OrdinalIgnoreCase)));
            // Add Other Clients as needed
            //return null;
        }
    }
}
