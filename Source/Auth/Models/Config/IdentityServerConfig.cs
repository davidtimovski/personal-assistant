using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;

namespace Auth.Models.Config
{
    public static class IdentityServerConfig
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email()
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("personal-assistant-api", "Personal Assistant")
            };

        public static IEnumerable<Client> GetClients(IConfiguration config)
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "to-do-assistant",
                    ClientName = "To Do Assistant",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RedirectUris = { $"{config["AppSettings:HostApplicationUrls:ToDoAssistant"]}/signin-oidc" },
                    PostLogoutRedirectUris = { config["AppSettings:HostApplicationUrls:ToDoAssistant"] },
                    AllowedCorsOrigins = { config["AppSettings:HostApplicationUrls:ToDoAssistant"] },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        "personal-assistant-api"
                    },
                    AccessTokenLifetime = 604800 // 1 week
                },
                new Client
                {
                    ClientId = "cooking-assistant",
                    ClientName = "Cooking Assistant",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RedirectUris = { $"{config["AppSettings:HostApplicationUrls:CookingAssistant"]}/signin-oidc" },
                    PostLogoutRedirectUris = { config["AppSettings:HostApplicationUrls:CookingAssistant"] },
                    AllowedCorsOrigins = { config["AppSettings:HostApplicationUrls:CookingAssistant"] },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        "personal-assistant-api"
                    },
                    AccessTokenLifetime = 604800 // 1 week
                },
                new Client
                {
                    ClientId = "accountant",
                    ClientName = "Accountant",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = false,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RedirectUris = { $"{config["AppSettings:HostApplicationUrls:Accountant"]}/signin-oidc" },
                    PostLogoutRedirectUris = { config["AppSettings:HostApplicationUrls:Accountant"] },
                    AllowedCorsOrigins = { config["AppSettings:HostApplicationUrls:Accountant"] },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        "personal-assistant-api"
                    },
                    AccessTokenLifetime = 604800 // 1 week
                }
            };
        }
    }
}
