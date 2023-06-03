using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.UnitTests.Builders;

internal class ControllerContextBuilder
{
    private readonly int _userId;
    private readonly string _auth0Id;

    internal ControllerContextBuilder()
    {
        _userId = 1;
        _auth0Id = "dummyAuth0Id";
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
                        new Claim("sub", _userId.ToString()),
                        new Claim(ClaimTypes.Name, _auth0Id),
                    }, "mock"))
            }
        };
    }
}
