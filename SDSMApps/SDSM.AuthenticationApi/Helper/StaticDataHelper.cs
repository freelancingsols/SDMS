using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDSM.AuthenticationApi.Helper
{
    public static class StaticDataHelper
    {
        public struct ClientIds
        {
            public const string EndUserWebApp = "sdsm.enduser.web.app";
            public const string B2BWebApp = "sdsm.b2b.web.app";
            public const string BackOfficeWebApp = "sdsm.backoffice.web.app";
            public const string DeliveryPartnerWebApp = "sdsm.deliverypartner.web.app";
            public const string VendorWebApp = "sdsm.vendor.web.app";
            public const string app2 = "sdsm.enduser.web.app";
        }
        public struct ClientNames
        {
            public const string app1 = "sdsm  Web Application";
            public const string app2 = "sdsm  Web Application";
        }
        public struct ApiNames
        {
            public const string GateWayApi = "sdsm.gateway.api";
            public const string ContentManagementApi = "sdsm.contentmanagement.api";
            //public const string BackOfficeWebAppe = "sdsm.backoffice.web.app";
            //public const string DeliveryPartnerWebApp = "sdsm.deliverypartner.web.app";
            //public const string VendorWebApp = "sdsm.vendor.web.app";
            //public const string app2 = "sdsm.enduser.web.app";
        }
        public struct ApiDescription
        {
            public const string app1 = "sdsm  api Application";
            public const string app2 = "sdsm  api Application";
        }
        public static List<Client> Clients { get; private set; }
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
            };
        }
        static StaticDataHelper()
        {
            Clients = new List<Client>
            {
            new Client
            {
                ClientId = ClientIds.EndUserWebApp,
                ClientName = ClientNames.app1,
                AllowedGrantTypes = GrantTypes.Implicit,
                ClientSecrets = { new Secret("02F97D49-18F8-4E20-AD8D-0EA51F3450A0".Sha256()) },
                RedirectUris = { $"http://localhost:5000/login-callback",$"http://localhost:5000/assets/login-callback-silent.html",$"http://localhost:5000/login-callback-popup",$"http://localhost:5000/login-callback-redirect" },
                FrontChannelLogoutUri = $"http://localhost:5000/logout-callback",
                BackChannelLogoutUri = $"http://localhost:5000/logout-callback",
                PostLogoutRedirectUris = { $"http://localhost:5000/logout-callback" },

                AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api.gateway"

                    },
                AllowOfflineAccess = true,
                AllowAccessTokensViaBrowser=true,
                //RefreshTokenUsage
                RequireConsent = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Absolute,
                // AccessTokenLifetime = 120,                 
                AbsoluteRefreshTokenLifetime = 157700000
            },
            new Client
            {
                ClientId = "sdsm.app.extranet",
                ClientName = "sdsm app Extranet Web Application",
                AllowedGrantTypes = GrantTypes.Hybrid,
                ClientSecrets = { new Secret("2C353098-643A-40B7-989C-C62582C47439".Sha256()) },
                RedirectUris = { $"{"appclient"}signin-oidc" },
                FrontChannelLogoutUri = $"{"appclient"}signout-oidc",
                PostLogoutRedirectUris = new List<string> { $"{"appclient"}signout-callback-oidc" },

                AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "api.gateway"
                  },
                AllowOfflineAccess = true,
                RequireConsent = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Absolute,
                AbsoluteRefreshTokenLifetime = 157700000
            },
            new Client
            {
                ClientId = "sdsm.app.b2b",
                ClientName = "sdsm app B2b Web Application",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RequireConsent = false,
                RedirectUris = {
                    $"_modelConfiguration.WebB2BEndpoint/assets/oidc-login-redirect.html",
                    $"WebB2BEndpointassets/silent-redirect.html"
                 },
                PostLogoutRedirectUris = { $"_modelConfiguration.WebB2BEndpoint?postLogout=true" },
                AllowedCorsOrigins = { $"_modelConfiguration.WebB2BEndpoint" },
                AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "sdsm.api.api.backoffice",
                        "sdsm.api.api.extranet",
                        "sdsm.api.api.usermanagement",
                        "sdsm.api.api.gateway"
                  },
                //IdentityTokenLifetime = 120,
                //AccessTokenLifetime = 120
            },
            new  Client
            {
                ClientId = "APIUser",
                ClientName = "API User Notification Service",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("6BC502B9-61D3-4D4F-A4EA-E175FE9EFA5D".Sha256()) },


                AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "sdsm.api.api.extranet",
                        "sdsm.api.api.gateway"
                    },
                AllowOfflineAccess = true,
                RequireConsent = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Absolute,
                AbsoluteRefreshTokenLifetime = 157700000
                //AccessTokenLifetime = 30 

            },
            new Client
            {
                ClientId = "BJUser",
                ClientName = "Background Job User Notification Service",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("222E5CDC-A34A-4196-BA36-E48C9906155B".Sha256()) },


                AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "sdsm.api.api.gateway"
                    },
                AllowOfflineAccess = true,
                RequireConsent = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Absolute,
                AbsoluteRefreshTokenLifetime = 157700000
                //AccessTokenLifetime = 30 

            }
            // non-interactive
            //    new Client
            //    {
            //        ClientId = "m2m",
            //        ClientName = "Machine to machine (client credentials)",
            //        ClientSecrets = { new Secret("secret".Sha256()) },

            //        AllowedGrantTypes = GrantTypes.ClientCredentials,
            //        AllowedScopes = { "api", "api.scope1", "api.scope2", "scope2", "policyserver.runtime", "policyserver.management" },
            //    },
            //    new Client
            //    {
            //        ClientId = "m2m.short",
            //        ClientName = "Machine to machine with short access token lifetime (client credentials)",
            //        ClientSecrets = { new Secret("secret".Sha256()) },

            //        AllowedGrantTypes = GrantTypes.ClientCredentials,
            //        AllowedScopes = { "api", "api.scope1", "api.scope2", "scope2" },
            //        AccessTokenLifetime = 75
            //    },

            //    // interactive
            //    new Client
            //    {
            //        ClientId = "interactive.confidential",
            //        ClientName = "Interactive client (Code with PKCE)",

            //        RedirectUris = { "https://notused" },
            //        PostLogoutRedirectUris = { "https://notused" },

            //        ClientSecrets = { new Secret("secret".Sha256()) },

            //        AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
            //        AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" },

            //        AllowOfflineAccess = true,
            //        RefreshTokenUsage = TokenUsage.ReUse,
            //        RefreshTokenExpiration = TokenExpiration.Sliding
            //    },
            //    new Client
            //    {
            //        ClientId = "interactive.confidential.short",
            //        ClientName = "Interactive client with short token lifetime (Code with PKCE)",

            //        RedirectUris = { "https://notused" },
            //        PostLogoutRedirectUris = { "https://notused" },

            //        ClientSecrets = { new Secret("secret".Sha256()) },
            //        RequireConsent = false,

            //        AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
            //        RequirePkce = true,
            //        AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" },

            //        AllowOfflineAccess = true,
            //        RefreshTokenUsage = TokenUsage.ReUse,
            //        RefreshTokenExpiration = TokenExpiration.Sliding,

            //        AccessTokenLifetime = 75
            //    },

            //    new Client
            //    {
            //        ClientId = "interactive.public",
            //        ClientName = "Interactive client (Code with PKCE)",

            //        RedirectUris = { "https://notused" },
            //        PostLogoutRedirectUris = { "https://notused" },

            //        RequireClientSecret = false,

            //        AllowedGrantTypes = GrantTypes.Code,
            //        AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" },

            //        AllowOfflineAccess = true,
            //        RefreshTokenUsage = TokenUsage.OneTimeOnly,
            //        RefreshTokenExpiration = TokenExpiration.Sliding
            //    },
            //    new Client
            //    {
            //        ClientId = "interactive.public.short",
            //        ClientName = "Interactive client with short token lifetime (Code with PKCE)",

            //        RedirectUris = { "https://notused" },
            //        PostLogoutRedirectUris = { "https://notused" },

            //        RequireClientSecret = false,

            //        AllowedGrantTypes = GrantTypes.Code,
            //        AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" },

            //        AllowOfflineAccess = true,
            //        RefreshTokenUsage = TokenUsage.OneTimeOnly,
            //        RefreshTokenExpiration = TokenExpiration.Sliding,

            //        AccessTokenLifetime = 75
            //    },

            //    new Client
            //    {
            //        ClientId = "device",
            //        ClientName = "Device Flow Client",

            //        AllowedGrantTypes = GrantTypes.DeviceFlow,
            //        RequireClientSecret = false,

            //        AllowOfflineAccess = true,
            //        RefreshTokenUsage = TokenUsage.OneTimeOnly,
            //        RefreshTokenExpiration = TokenExpiration.Sliding,

            //        AllowedScopes = { "openid", "profile", "email", "api", "api.scope1", "api.scope2", "scope2" }
            //    },

            //    // oidc login only
            //    new Client
            //    {
            //        ClientId = "login",

            //        RedirectUris = { "https://notused" },
            //        PostLogoutRedirectUris = { "https://notused" },

            //        AllowedGrantTypes = GrantTypes.Implicit,
            //        AllowedScopes = { "openid", "profile", "email" },
            //    }
        };
        }
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                // backward compat
                new ApiScope("api"),
                
                // more formal
                new ApiScope("api.scope1"),
                new ApiScope("api.scope2"),
                
                // scope without a resource
                new ApiScope("scope2"),
                
                // policyserver
                new ApiScope("policyserver.runtime"),
                new ApiScope("policyserver.management")
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource( ApiNames.GateWayApi, ApiDescription.app1)
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },

                    Scopes = { ApiNames.GateWayApi }
                },

                // PolicyServer demo (audience should match scope)
                //new ApiResource("policyserver.runtime")
                //{
                //    Scopes = { "policyserver.runtime" }
                //},
                //new ApiResource("policyserver.management")
                //{
                //    Scopes = { "policyserver.runtime" }
                //}
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return Clients;
        }
    }
}
