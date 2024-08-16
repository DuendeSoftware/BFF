using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Duende.Bff.Blazor;

public class CaptureManagementClaimsCookieEvents : CookieAuthenticationEvents
{
    private readonly IClaimsService _claimsService;

    public CaptureManagementClaimsCookieEvents(IClaimsService claimsService)
    {
        _claimsService = claimsService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var managementClaims = await _claimsService.GetManagementClaimsAsync(
            context.Request.PathBase,
            context.Principal, context.Properties);

        if (context.Principal?.Identity is ClaimsIdentity id)
        {

            foreach (var claim in managementClaims)
            {
                if (context.Principal.Claims.Any(c => c.Type == claim.type) != true)
                {
                    id.AddClaim(new Claim(claim.type, claim.value?.ToString() ?? string.Empty));
                }
            }
        }
    }
}