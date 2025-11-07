using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDMS.AuthenticationApi.Helper;

namespace SDMS.AuthenticationApi.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class OidcConfigurationController : ControllerBase
    {
        //public OidcConfigurationController(IClientRequestParametersProvider clientRequestParametersProvider)
        //{
        //    ClientRequestParametersProvider = clientRequestParametersProvider;
        //}

        //public IClientRequestParametersProvider ClientRequestParametersProvider { get; }

        //[HttpGet("_configuration/{clientId}")]
        //public IActionResult GetClientRequestParameters([FromRoute] string clientId)
        //{
        //    var parameters = ClientRequestParametersProvider.GetClientParameters(HttpContext, clientId);
        //    return Ok(parameters);
        //}

        [HttpGet("_configuration/{clientId}")]
        public IActionResult GetClientRequestParameters([FromRoute] string clientId)
        {
            var client = StaticDataHelper.Clients.FirstOrDefault(x => x.ClientId.Equals(clientId, StringComparison.OrdinalIgnoreCase));
            string scope = string.Empty;
            client.AllowedScopes.ToList().ForEach(x => { scope = x + " "; });
            scope.Trim();
            return Ok
                (
                    new 
                    {
                        client.ClientId,
                        RedirectUri=client.RedirectUris.First(),
                        PostLogoutRedirectUri = client.PostLogoutRedirectUris.First(),
                        //response_type,
                        Scope= scope                    }
                );
        }
    }
}
