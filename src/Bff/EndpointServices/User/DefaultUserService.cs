// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.Bff.Logging;
using Microsoft.Extensions.Options;

namespace Duende.Bff;

/// <summary>
/// Service for handling user requests
/// </summary>
public class DefaultUserService : IUserService
{
    /// <summary>
    /// The claims service
    /// </summary>
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
    /// <param name="claims"></param>
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
            // In blazor, it is sometimes necessary to copy management claims
            // into the session. So, we don't want duplicate mgmt claims.
            // Instead, they should overwrite the existing mgmt claims (in case
            // they changed when the session slid, etc)
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

/// <summary>
/// Serialization-friendly claim
/// </summary>
/// <param name="type"></param>
/// <param name="value"></param>
public record ClaimRecord(string type, object value);