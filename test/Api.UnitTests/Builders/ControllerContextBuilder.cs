using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PersonalAssistant.Api.UnitTests.Builders
{
    public class ControllerContextBuilder
    {
        private int userId;

        public ControllerContextBuilder()
        {
            userId = 1;
        }

        public ControllerContextBuilder WithUserId(int newUserId)
        {
            userId = newUserId;
            return this;
        }

        public ControllerContext Build()
        {
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        new Claim[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                        }, "mock"))
                }
            };
        }
    }
}
