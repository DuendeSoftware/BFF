// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Duende.Bff;

/// <inheritdoc />
public class DefaultClaimsService : IClaimsService
{
    private readonly BffOptions Options;
    
    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="options"></param>
    public DefaultClaimsService(IOptions<BffOptions> options)
    {
        Options = options.Value;
    }

    /// <inheritdoc />
    public Task<IEnumerable<ClaimRecord>> GetManagementClaimsAsync(PathString pathBase, ClaimsPrincipal? principal, AuthenticationProperties? properties)
    {
        var claims = new List<ClaimRecord>();

        var sessionId = principal?.FindFirst(JwtClaimTypes.SessionId)?.Value;
        if (!String.IsNullOrWhiteSpace(sessionId))
        {
            claims.Add(new ClaimRecord(
                Constants.ClaimTypes.LogoutUrl,
                pathBase + Options.LogoutPath.Value + $"?sid={UrlEncoder.Default.Encode(sessionId)}"));
        }

        if (properties != null)
        {
            if (properties.ExpiresUtc.HasValue)
            {
                var expiresInSeconds =
                    properties.ExpiresUtc.Value.Subtract(DateTimeOffset.UtcNow).TotalSeconds;
                claims.Add(new ClaimRecord(
                    Constants.ClaimTypes.SessionExpiresIn,
                    Math.Round(expiresInSeconds)));
            }

            if (properties.Items.TryGetValue(OpenIdConnectSessionProperties.SessionState, out var sessionState) && sessionState is not null)
            {
                claims.Add(new ClaimRecord(Constants.ClaimTypes.SessionState, sessionState));
            }
        }

        return Task.FromResult<IEnumerable<ClaimRecord>>(claims);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ClaimRecord>> GetUserClaimsAsync(ClaimsPrincipal? principal, AuthenticationProperties? properties) => 
        Task.FromResult(principal?.Claims.Select(x => new ClaimRecord(x.Type, x.Value)) ?? Enumerable.Empty<ClaimRecord>());
}
