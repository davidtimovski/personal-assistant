using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Infrastructure.Identity;

public static class Auth0ManagementUtil
{
    private static string Domain;
    private static string AccessToken;
    private static DateTime? Expires;

    public static async Task InitializeAsync(HttpClient httpClient, Auth0ManagementUtilConfig config)
    {
        Domain = config.Domain;

        if (Expires.HasValue && Expires > DateTime.UtcNow.AddMinutes(1))
        {
            return;
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{Domain}/oauth/token"));
        requestMessage.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", config.ClientId),
            new KeyValuePair<string, string>("client_secret", config.ClientSecret),
            new KeyValuePair<string, string>("audience", $"https://{config.Domain}/api/v2/")
        });

        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        var result = JsonConvert.DeserializeObject<TokenResult>(await response.Content.ReadAsStringAsync());
        AccessToken = result.access_token;
        Expires = DateTime.UtcNow.AddSeconds(result.expires_in);
    }

    public static async Task<User> GetUserAsync(HttpClient httpClient, string auth0Id)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://{Domain}/api/v2/users/{auth0Id}?fields=email,name,user_metadata"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        var result = JsonConvert.DeserializeObject<Auth0User>(await response.Content.ReadAsStringAsync());

        return new User
        {
            Email = result.email,
            Name = result.name,
            Language = result.user_metadata.language,
            ImageUri = result.user_metadata.image_uri
        };
    }

    public static async Task UpdateUserAsync(HttpClient httpClient, string auth0Id, User user)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, new Uri($"https://{Domain}/api/v2/users/{auth0Id}"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var profile = new Auth0User
        {
            name = user.Name,
            user_metadata = new Auth0UserProfileMetadata
            {
                language = user.Language,
                image_uri = user.ImageUri
            }
        };
        requestMessage.Content = new StringContent(JsonConvert.SerializeObject(profile), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();
    }

    public static async Task ResetPasswordAsync(HttpClient httpClient, string clientId, string email)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{Domain}/dbconnections/change_password"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        requestMessage.Content = new StringContent(JsonConvert.SerializeObject(new
        {
            client_id = clientId,
            email = email,
            connection = "Username-Password-Authentication"
        }), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();
    }
}

public struct Auth0ManagementUtilConfig
{
    public Auth0ManagementUtilConfig(string domain, string clientId, string clientSecret)
    {
        Domain = domain;
        ClientId = clientId;
        ClientSecret = clientSecret;
    }

    public string Domain { get; }
    public string ClientId { get; }
    public string ClientSecret { get; }
}

internal class Auth0User
{
    public string email { get; set; }
    public string name { get; set; }
    public Auth0UserProfileMetadata user_metadata { get; set; }
}

internal class Auth0UserProfileMetadata
{
    public string language { get; set; }
    public string image_uri { get; set; }
}

internal class TokenResult
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public string scope { get; set; }
    public string token_type { get; set; }
}
