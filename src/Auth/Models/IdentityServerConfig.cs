using System;
using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;

namespace Auth.Models;

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
            new ApiResource("personal-assistant-api", "Personal Assistant API")
            {
                Scopes = { "personal-assistant-api" }
            },
            new ApiResource("personal-assistant-gateway", "Personal Assistant Gateway")
            {
                Scopes = { "personal-assistant-gateway" }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("personal-assistant-api"),
            new ApiScope("personal-assistant-gateway")
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
                RedirectUris = { $"{config["Urls:ToDoAssistant"]}/signin-oidc" },
                PostLogoutRedirectUris = { config["Urls:ToDoAssistant"] },
                AllowedCorsOrigins = { config["Urls:ToDoAssistant"] },
                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Email,
                    "personal-assistant-api",
                    "personal-assistant-gateway"
                },
                AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds
            },
            new Client
            {
                ClientId = "cooking-assistant",
                ClientName = "Cooking Assistant",
                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = false,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = { $"{config["Urls:CookingAssistant"]}/signin-oidc" },
                PostLogoutRedirectUris = { config["Urls:CookingAssistant"] },
                AllowedCorsOrigins = { config["Urls:CookingAssistant"] },
                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Email,
                    "personal-assistant-api",
                    "personal-assistant-gateway"
                },
                AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds
            },
            new Client
            {
                ClientId = "accountant",
                ClientName = "Accountant",
                AllowedGrantTypes = GrantTypes.Code,
                RequireConsent = false,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = { $"{config["Urls:Accountant"]}/signin-oidc" },
                PostLogoutRedirectUris = { config["Urls:Accountant"] },
                AllowedCorsOrigins = { config["Urls:Accountant"] },
                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Email,
                    "personal-assistant-api",
                    "personal-assistant-gateway"
                },
                AccessTokenLifetime = (int)TimeSpan.FromDays(30).TotalSeconds
            }
        };
    }
}
