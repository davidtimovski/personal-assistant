using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Identity;

namespace Auth.Services;

public class IdentityServerProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityServerProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);

        var claims = new List<Claim>
        {
            new Claim("name", user.Email),
            new Claim("name2", user.Name)
        };

        context.IssuedClaims.AddRange(claims);
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);

        context.IsActive = user != null;
    }
}
