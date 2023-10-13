using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using Core.Infrastructure.Configuration;
using Sentry;

namespace Core.Infrastructure.Identity;

public static class Auth0Proxy
{
    private static Auth0ManagementUtilConfig Config = null!;
    private static string? AccessToken;
    private static DateTime? Expires;

    public static async Task InitializeAsync(HttpClient httpClient, Auth0ManagementUtilConfig config, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(Auth0Proxy)}.{nameof(InitializeAsync)}");

        Config = config;

        if (Expires.HasValue && Expires > DateTime.UtcNow.AddMinutes(1))
        {
            return;
        }

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{Config.Domain}/oauth/token"));
        requestMessage.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", config.ClientId),
            new KeyValuePair<string, string>("client_secret", config.ClientSecret),
            new KeyValuePair<string, string>("audience", $"https://{config.Domain}/api/v2/")
        });

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<TokenResult>(content);
        if (result is null)
        {
            throw new SerializationException($"Could not deserialize {nameof(TokenResult)} from content");
        }

        AccessToken = result.access_token;
        Expires = DateTime.UtcNow.AddSeconds(result.expires_in);

        metric.Finish();
    }

    public static async Task<Auth0User> GetUserAsync(HttpClient httpClient, string auth0Id, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(Auth0Proxy)}.{nameof(GetUserAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://{Config.Domain}/api/v2/users/{auth0Id}?fields=email,name,user_metadata"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<Auth0User>(content);
        if (result is null)
        {
            throw new SerializationException($"Could not deserialize {nameof(Auth0User)} from content: {content}");
        }

        metric.Finish();

        return result;
    }

    public static async Task<string> RegisterUserAsync(HttpClient httpClient, string email, string password, string name, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(Auth0Proxy)}.{nameof(RegisterUserAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{Config.Domain}/api/v2/users"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        var createModel = new CreateUserPayload(
                email.Trim(),
                false,
                password,
                name.Trim()
            );
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(createModel), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (content.Contains("PasswordStrengthError"))
            {
                throw new PasswordTooWeakException();
            }

            throw new Auth0Exception(content);
        }

        var result = JsonSerializer.Deserialize<CreateUserResult>(content);
        if (result is null)
        {
            throw new SerializationException($"Could not deserialize {nameof(CreateUserResult)} from content: {content}");
        }

        metric.Finish();

        return result.user_id;
    }

    public static async Task DeleteUserAsync(HttpClient httpClient, string auth0Id, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(Auth0Proxy)}.{nameof(DeleteUserAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, new Uri($"https://{Config.Domain}/api/v2/users/{auth0Id}"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        metric.Finish();
    }

    public static async Task UpdateNameAsync(HttpClient httpClient, string auth0Id, string name, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(Auth0Proxy)}.{nameof(UpdateNameAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, new Uri($"https://{Config.Domain}/api/v2/users/{auth0Id}"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(new
        {
            name = name.Trim()
        }), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        metric.Finish();
    }

    public static async Task ResetPasswordAsync(HttpClient httpClient, string email, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(Auth0Proxy)}.{nameof(ResetPasswordAsync)}");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"https://{Config.Domain}/dbconnections/change_password"));
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);

        requestMessage.Content = new StringContent(JsonSerializer.Serialize(new
        {
            client_id = Config.ClientId,
            email = email,
            connection = "Username-Password-Authentication"
        }), Encoding.UTF8, "application/json");

        using var response = await httpClient.SendAsync(requestMessage, cancellationToken);
        response.EnsureSuccessStatusCode();

        metric.Finish();
    }
}
