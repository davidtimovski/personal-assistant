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

        if (Expires.HasValue && Expires < DateTime.UtcNow.AddMinutes(-1))
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

    public static async Task<UserProfile> GetUserProfileAsync(HttpClient httpClient, string auth0Id)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://{Domain}/api/v2/users/{auth0Id}?fields=email,name,user_metadata"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        return JsonConvert.DeserializeObject<UserProfile>(await response.Content.ReadAsStringAsync());
    }

    public static async Task UpdateUserProfileAsync(HttpClient httpClient, string auth0Id, UserProfile profile)
    {
        using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, new Uri($"https://{Domain}/api/v2/users/{auth0Id}"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

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

public class UserProfile
{
    public string email { get; set; }
    public string name { get; set; }
    public UserProfileMetadata user_metadata { get; set; }
}

public class UserProfileMetadata
{
    public string Language { get; set; }
    public string ImageUri { get; set; }
}

internal class TokenResult
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public string scope { get; set; }
    public string token_type { get; set; }
}
