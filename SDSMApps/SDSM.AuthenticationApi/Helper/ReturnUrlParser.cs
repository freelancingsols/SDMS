using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SDSM.AuthenticationApi.Extensions;

namespace SDSM.AuthenticationApi.Helper
{
    public class ReturnUrlParser: IReturnUrlParser
    {
        public static class ProtocolRoutePaths
        {
            public const string Authorize = "connect/authorize";
            public const string AuthorizeCallback = Authorize + "/callback";
        }

        private readonly IAuthorizeRequestValidator _validator;
        private readonly IUserSession _userSession;

        public ReturnUrlParser(
            IAuthorizeRequestValidator validator,
            IUserSession userSession)
        {
            _validator = validator;
            _userSession = userSession;
        }

        public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
        {
            if (IsValidReturnUrl(returnUrl))
            {
                var parameters = returnUrl.ReadQueryStringAsNameValueCollection();
                var user = await _userSession.GetUserAsync();
                var result = await _validator.ValidateAsync(parameters, user);
                if (!result.IsError)
                {
                    return result.ValidatedRequest.ToAuthorizationRequest();
                }
            }
            return null;
        }

        public bool IsValidReturnUrl(string returnUrl)
        {
            // had to add returnUrl.StartsWith("http://localhost:5000")
            // because when UI and API are not on the same host, the URL is not local
            // the condition here should be changed to either use configuration or just match domain
            if (returnUrl.IsLocalUrl() || returnUrl.StartsWith("http://localhost:5000"))
            {
                var index = returnUrl.IndexOf('?');
                if (index >= 0)
                {
                    returnUrl = returnUrl.Substring(0, index);
                }

                if (returnUrl.EndsWith(ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
                    returnUrl.EndsWith(ProtocolRoutePaths.AuthorizeCallback, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
