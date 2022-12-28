using System.Collections.Generic;
using IdentityModel;
using System.Security.Claims;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Washme.Identity.Service.Data
{
    public class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                new IdentityResource[]
                {
                new IdentityResources.OpenId()
                };

        public static IEnumerable<Client> Clients = new List<Client>
        {
            new Client
            {
                ClientId = "washmeauth",

                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                //allow refresh token
                AllowOfflineAccess = true,

                // secret for authentication
                ClientSecrets =
                {
                   new Secret("ugeOi69O3azvBp3bY6Fkooyq9mgtWrTV".Sha256())
                },
                RequireClientSecret = false,
                RequireConsent = false,
                RedirectUris = new List<string> { "http://localhost:3006/signin-callback.html" },
                PostLogoutRedirectUris = new List<string> { "http://localhost:3006/" },
                AllowedScopes = { "AppsScope" },
                AllowedCorsOrigins = new List<string>
                {
                    "http://localhost:3006",
                },
                AccessTokenLifetime = 600
            },
            new Client
                    {
                        ClientId = "washmeauthapps",

                        //allow refresh token
                        AllowOfflineAccess = true,

                        AllowedGrantTypes = new []
                        {
                            GrantType.ResourceOwnerPassword
                        },

                        // secret for authentication
                        ClientSecrets =
                        {
                            new Secret("5put3Thk9N2ohLfqq6AENuJFC3dICTpr".Sha256())
                        },

                        AccessTokenLifetime = 60*5,
                        AbsoluteRefreshTokenLifetime = 60*30,
                        SlidingRefreshTokenLifetime = 60*15,

                        // scopes that client has access to
                        AllowedScopes = { "AppsScope"}

                    }
        };

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "Niluk",
                    Password = "password1",
                    Claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.Email,"niluk@gmail.com")
                    }
                }
            };
        }

        public static IEnumerable<ApiScope> ApiScopes = new List<ApiScope>
        {
            new ApiScope(name: "AppsScope",   displayName: "Apps scope"),
        };
    }
}
