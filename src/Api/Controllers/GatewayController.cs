using System.Text;
using System.Text.Json;
using Api.Models;
using Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[EnableCors("AllowAllApps")]
[Route("api/[controller]")]
public class GatewayController : BaseController
{
    private readonly IHttpClientFactory _httpClientFactory;

    public GatewayController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IHttpClientFactory httpClientFactory) : base(userIdLookup, usersRepository)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ClientLog clientLog)
    {
        clientLog.UserId = UserId;

        using HttpClient httpClient = _httpClientFactory.CreateClient(clientLog.Service);

        var stringContent = new StringContent(JsonSerializer.Serialize(clientLog), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.PostAsync(clientLog.Url, stringContent);
        response.EnsureSuccessStatusCode();

        return StatusCode((int)response.StatusCode);
    }
}
