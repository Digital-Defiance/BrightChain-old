using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources() => new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<Client> GetClients() => new List<Client>
            {
                new Client
                {
                    ClientId = "AspNetCoreIdentity",
                    ClientName = "AspNetCoreIdentity Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris =           { "http://localhost:5000" },
                    PostLogoutRedirectUris = { "http://localhost:5000" },
                    AllowedCorsOrigins =     { "http://localhost:5000" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "SocialAPI"
                    }
                }
            };

        public static IEnumerable<ApiResource> GetApis() => new List<ApiResource>
            {
                new ApiResource("SocialAPI", "Social Network API")
            };
    }
}
