using System;
using System.Security.Claims;

namespace Infrastructure.Identity;

public static class IdentityHelper
{
    public static int GetUserId(ClaimsPrincipal principal)
    {
        string id = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
        return Convert.ToInt32(id);
    }

    public static string GetUserName(ClaimsPrincipal principal)
    {
        return principal.FindFirst("name2").Value;
    }
}