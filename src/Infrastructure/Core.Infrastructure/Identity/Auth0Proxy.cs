using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Core.Application.Services;
using Sentry;

namespace Core.Infrastructure.Identity;

public static class Auth0Proxy
{
    private static string Domain;
    private static string AccessToken;
    private static DateTime? Expires;

    public static async Task InitializeAsync(HttpClient httpClient, Auth0ManagementUtilConfig config, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(Auth0Proxy)}.{nameof(InitializeAsync)}");

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

        var result = JsonSerializer.Deserialize<TokenResult>(await response.Content.ReadAsStringAsync());
        AccessToken = result.access_token;
        Expires = DateTime.UtcNow.AddSeconds(result.expires_in);

        span.Finish();
    }

    public static async Task<Auth0User> GetUserAsync(HttpClient httpClient, string auth0Id, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(Auth0Proxy)}.{nameof(GetUserAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://{Domain}/api/v2/users/{auth0Id}?fields=email,name,user_metadata"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        var result = JsonSerializer.Deserialize<Auth0User>(await response.Content.ReadAsStringAsync());

        span.Finish();

        return result;
    }

    public static async Task<string> RegisterUserAsync(HttpClient httpClient, string email, string password, string name, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(Auth0Proxy)}.{nameof(RegisterUserAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{Domain}/api/v2/users"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var createModel = new CreateUserPayload(
                email.Trim(),
                false,
                password,
                name.Trim()
            );
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(createModel), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (content.Contains("PasswordStrengthError"))
            {
                throw new PasswordTooWeakException();
            }

            throw new Auth0Exception(content);
        }

        var result = JsonSerializer.Deserialize<CreateUserResult>(await response.Content.ReadAsStringAsync());

        span.Finish();

        return result.user_id;
    }

    public static async Task DeleteUserAsync(HttpClient httpClient, string auth0Id, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(Auth0Proxy)}.{nameof(DeleteUserAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, new Uri($"https://{Domain}/api/v2/users/{auth0Id}"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        span.Finish();
    }

    public static async Task UpdateNameAsync(HttpClient httpClient, string auth0Id, string name, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(Auth0Proxy)}.{nameof(UpdateNameAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, new Uri($"https://{Domain}/api/v2/users/{auth0Id}"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(new
        {
            name = name.Trim()
        }), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        span.Finish();
    }

    public static async Task ResetPasswordAsync(HttpClient httpClient, string clientId, string email, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(Auth0Proxy)}.{nameof(ResetPasswordAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{Domain}/dbconnections/change_password"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(new
        {
            client_id = clientId,
            email = email,
            connection = "Username-Password-Authentication"
        }), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        span.Finish();
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
