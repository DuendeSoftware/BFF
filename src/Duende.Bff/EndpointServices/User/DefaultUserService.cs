// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.Bff.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Duende.Bff;

/// <summary>
/// Service for handling user requests
/// </summary>
public class DefaultUserService : IUserService
{
    protected readonly IClaimsService Claims;
    
    /// <summary>
    /// The options
    /// </summary>
    protected readonly BffOptions Options;
        
    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="loggerFactory"></param>
    public DefaultUserService(IClaimsService claims, IOptions<BffOptions> options, ILoggerFactory loggerFactory)
    {
        Claims = claims;
        Options = options.Value;
        Logger = loggerFactory.CreateLogger(LogCategories.ManagementEndpoints);
    }

    /// <inheritdoc />
    public virtual async Task ProcessRequestAsync(HttpContext context)
    {
        Logger.LogDebug("Processing user request");
        
        context.CheckForBffMiddleware(Options);

        var result = await context.AuthenticateAsync();

        if (!result.Succeeded)
        {
            if (Options.AnonymousSessionResponse == AnonymousSessionResponse.Response200)
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("null", Encoding.UTF8);
            }
            else
            {
                context.Response.StatusCode = 401;
            }

            Logger.LogDebug("User endpoint indicates the user is not logged in, using status code {code}", context.Response.StatusCode);
        }
        else
        {
            // In blazor, it is sometimes necessary to copy management claims into the session.
            // So, we don't want duplicate mgmt claims. Instead, they should overwrite the existing mgmt claims
            // (in case they changed when the session slide, etc)
            var claims = (await GetUserClaimsAsync(result)).ToList();
            var mgmtClaims = await GetManagementClaimsAsync(context, result);

            foreach (var claim in mgmtClaims)
            {
                claims.RemoveAll(c => c.type == claim.type);
                claims.Add(claim);
            }

            var json = JsonSerializer.Serialize(claims);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json, Encoding.UTF8);

            Logger.LogTrace("User endpoint indicates the user is logged in with claims {claims}", claims);
        }
    }

    /// <summary>
    /// Collect user-centric claims
    /// </summary>
    /// <param name="authenticateResult"></param>
    /// <returns></returns>
    protected virtual Task<IEnumerable<ClaimRecord>> GetUserClaimsAsync(AuthenticateResult authenticateResult) => 
        Claims.GetUserClaimsAsync(authenticateResult.Principal, authenticateResult.Properties);

    /// <summary>
    /// Collect management claims
    /// </summary>
    /// <param name="context"></param>
    /// <param name="authenticateResult"></param>
    /// <returns></returns>
    protected virtual Task<IEnumerable<ClaimRecord>> GetManagementClaimsAsync(HttpContext context, AuthenticateResult authenticateResult)
    {
        return Claims.GetManagementClaimsAsync(context.Request.PathBase, authenticateResult.Principal, authenticateResult.Properties);
    }
}

public interface IClaimsService
{
    Task<IEnumerable<ClaimRecord>> GetUserClaimsAsync(ClaimsPrincipal? principal, AuthenticationProperties? properties);
    Task<IEnumerable<ClaimRecord>> GetManagementClaimsAsync(PathString pathBase, ClaimsPrincipal? principal, AuthenticationProperties? properties);
}

public class DefaultClaimsService : IClaimsService
{
    private readonly BffOptions Options;

    public DefaultClaimsService(IOptions<BffOptions> options)
    {
        Options = options.Value;
    }

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

    public Task<IEnumerable<ClaimRecord>> GetUserClaimsAsync(ClaimsPrincipal? principal, AuthenticationProperties? properties) => 
        Task.FromResult(principal?.Claims.Select(x => new ClaimRecord(x.Type, x.Value)) ?? Enumerable.Empty<ClaimRecord>());
}

/// <summary>
/// Serialization-friendly claim
/// </summary>
/// <param name="type"></param>
/// <param name="value"></param>
public record ClaimRecord(string type, object value);