// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Duende.Bff.Blazor;

/// <summary>
/// This <see cref="CookieAuthenticationEvents"/> subclass invokes the BFF <see
/// cref="IClaimsService"/> to retrieve management claims and add them to the
/// session. This is useful in interactive render modes where components are
/// initialled rendered server side. 
/// </summary>
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