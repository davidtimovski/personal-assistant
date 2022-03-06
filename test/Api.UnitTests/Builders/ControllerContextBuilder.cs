using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.UnitTests.Builders;

public class ControllerContextBuilder
{
    private readonly int _userId;

    public ControllerContextBuilder()
    {
        _userId = 1;
    }

    public ControllerContext Build()
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
                    }, "mock"))
            }
        };
    }
}
