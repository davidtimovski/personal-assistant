using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.UnitTests.Builders;

internal class ControllerContextBuilder
{
    private readonly int _userId;

    internal ControllerContextBuilder()
    {
        _userId = 1;
    }

    internal ControllerContext Build()
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim("sub", _userId.ToString())
                    }, "mock"))
            }
        };
    }
}
