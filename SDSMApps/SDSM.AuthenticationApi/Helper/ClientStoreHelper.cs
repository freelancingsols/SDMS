using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDSM.AuthenticationApi.Helper
{
    public class ClientStoreHelper:IClientStore
    {
        public Task<Client> FindClientByIdAsync(string clientId)
        {
            if (clientId == "xyz.web")
            {
                return Task.FromResult(GetWebAppClient());
            }
            else 
            {
                return null;
            }
            // Add Other Clients as needed
            //return null;
        }
        private Client GetWebAppClient()
        {
            return new Client
            {
                ClientId = "mg.jarvis.web",
                ClientName = "MG Jarvis Web Application",
                AllowedGrantTypes = GrantTypes.Hybrid,
                ClientSecrets = { new Secret("02F97D49-18F8-4E20-AD8D-0EA51F3450A0".Sha256()) },
                RedirectUris = { $"{"appclientendpoint"}signin-oidc" },
                FrontChannelLogoutUri = $"{"appclientendpoint"}signout-oidc",
                //   BackChannelLogoutUri = $"{_appSettings.BaserUrls.Web}logout",
                PostLogoutRedirectUris = new List<string> { $"{"appclientendpoint"}signout-callback-oidc" },

                AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api.gateway"

                    },
                AllowOfflineAccess = true,
                //AllowAccessTokensViaBrowser=true,
                //RefreshTokenUsage
                RequireConsent = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Absolute,
                // AccessTokenLifetime = 120,                 
                AbsoluteRefreshTokenLifetime = 157700000
            };
        }

        //private Client GetJarvisExtranetMvcClient()
        //{
        //    return new Client
        //    {
        //        ClientId = "mg.jarvis.extranet",
        //        ClientName = "MG Jarvis Extranet Web Application",
        //        AllowedGrantTypes = GrantTypes.Hybrid,
        //        ClientSecrets = { new Secret("2C353098-643A-40B7-989C-C62582C47439".Sha256()) },
        //        RedirectUris = { $"{"appclient"}signin-oidc" },
        //        FrontChannelLogoutUri = $"{"appclient"}signout-oidc",
        //        PostLogoutRedirectUris = new List<string> { $"{"appclient"}signout-callback-oidc" },

        //        AllowedScopes = new List<string>
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                "api.gateway"
        //          },
        //        AllowOfflineAccess = true,
        //        RequireConsent = false,
        //        AlwaysIncludeUserClaimsInIdToken = true,
        //        RefreshTokenUsage = TokenUsage.ReUse,
        //        RefreshTokenExpiration = TokenExpiration.Absolute,
        //        AbsoluteRefreshTokenLifetime = 157700000
        //    };
        //}

        //private Client GetJarvisB2bSpaClient()
        //{
        //    return new Client
        //    {
        //        ClientId = "mg.jarvis.b2b",
        //        ClientName = "MG Jarvis B2b Web Application",
        //        AllowedGrantTypes = GrantTypes.Implicit,
        //        AllowAccessTokensViaBrowser = true,
        //        RequireConsent = false,
        //        RedirectUris = {
        //            $"{_modelConfiguration.WebB2BEndpoint}assets/oidc-login-redirect.html",
        //            $"{_modelConfiguration.WebB2BEndpoint}assets/silent-redirect.html"
        //         },
        //        PostLogoutRedirectUris = { $"{_modelConfiguration.WebB2BEndpoint}?postLogout=true" },
        //        AllowedCorsOrigins = { $"{_modelConfiguration.WebB2BEndpoint}" },
        //        AllowedScopes = new List<string>
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                "mg.jarvis.api.backoffice",
        //                "mg.jarvis.api.extranet",
        //                "mg.jarvis.api.usermanagement",
        //                "mg.jarvis.api.gateway"
        //          },
        //        //IdentityTokenLifetime = 120,
        //        //AccessTokenLifetime = 120
        //    };
        //}

        //private Client GetAPIClient()
        //{
        //    return new Client
        //    {
        //        ClientId = "APIUser",
        //        ClientName = "API User Notification Service",
        //        AllowedGrantTypes = GrantTypes.ClientCredentials,
        //        ClientSecrets = { new Secret("6BC502B9-61D3-4D4F-A4EA-E175FE9EFA5D".Sha256()) },


        //        AllowedScopes = new List<string>
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                "mg.jarvis.api.extranet",
        //                "mg.jarvis.api.gateway"
        //            },
        //        AllowOfflineAccess = true,
        //        RequireConsent = false,
        //        AlwaysIncludeUserClaimsInIdToken = true,
        //        RefreshTokenUsage = TokenUsage.ReUse,
        //        RefreshTokenExpiration = TokenExpiration.Absolute,
        //        AbsoluteRefreshTokenLifetime = 157700000
        //        //AccessTokenLifetime = 30 

        //    };
        //}

        //private Client GetBJUserClient()
        //{
        //    return new Client
        //    {
        //        ClientId = "BJUser",
        //        ClientName = "Background Job User Notification Service",
        //        AllowedGrantTypes = GrantTypes.ClientCredentials,
        //        ClientSecrets = { new Secret("222E5CDC-A34A-4196-BA36-E48C9906155B".Sha256()) },


        //        AllowedScopes = new List<string>
        //            {
        //                IdentityServerConstants.StandardScopes.OpenId,
        //                IdentityServerConstants.StandardScopes.Profile,
        //                "mg.jarvis.api.gateway"
        //            },
        //        AllowOfflineAccess = true,
        //        RequireConsent = false,
        //        AlwaysIncludeUserClaimsInIdToken = true,
        //        RefreshTokenUsage = TokenUsage.ReUse,
        //        RefreshTokenExpiration = TokenExpiration.Absolute,
        //        AbsoluteRefreshTokenLifetime = 157700000
        //        //AccessTokenLifetime = 30 

        //    };
        //}
    }
}
