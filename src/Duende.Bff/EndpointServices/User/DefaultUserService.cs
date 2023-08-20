﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
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
    public DefaultUserService(IOptions<BffOptions> options, ILoggerFactory loggerFactory)
    {
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
            var claims = new List<ClaimRecord>();
            claims.AddRange(await GetUserClaimsAsync(result));
            claims.AddRange(await GetManagementClaimsAsync(context, result));

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
    protected virtual ValueTask<IEnumerable<ClaimRecord>> GetUserClaimsAsync(AuthenticateResult authenticateResult)
    {
        return ValueTask.FromResult(authenticateResult.Principal?.Claims.Select(x => new ClaimRecord(x.Type, x.Value)) ?? Enumerable.Empty<ClaimRecord>());
    }

    /// <summary>
    /// Collect management claims
    /// </summary>
    /// <param name="context"></param>
    /// <param name="authenticateResult"></param>
    /// <returns></returns>
    protected virtual ValueTask<IEnumerable<ClaimRecord>> GetManagementClaimsAsync(HttpContext context, AuthenticateResult authenticateResult)
    {
        var claims = new List<ClaimRecord>();

        var pathBase = context.Request.PathBase;

        var sessionId = authenticateResult.Principal?.FindFirst(JwtClaimTypes.SessionId)?.Value;
        if (!String.IsNullOrWhiteSpace(sessionId))
        {
            claims.Add(new ClaimRecord(
                Constants.ClaimTypes.LogoutUrl,
                pathBase + Options.LogoutPath.Value + $"?sid={UrlEncoder.Default.Encode(sessionId)}"));
        }

        if (authenticateResult.Properties != null)
        {
            if (authenticateResult.Properties.ExpiresUtc.HasValue)
            {
                var expiresInSeconds =
                    authenticateResult.Properties.ExpiresUtc.Value.Subtract(DateTimeOffset.UtcNow).TotalSeconds;
                claims.Add(new ClaimRecord(
                    Constants.ClaimTypes.SessionExpiresIn,
                    Math.Round(expiresInSeconds)));
            }

            if (authenticateResult.Properties.Items.TryGetValue(OpenIdConnectSessionProperties.SessionState, out var sessionState) && sessionState is not null)
            {
                claims.Add(new ClaimRecord(Constants.ClaimTypes.SessionState, sessionState));
            }
        }

        return ValueTask.FromResult(claims as IEnumerable<ClaimRecord>);
    }
        
    /// <summary>
    /// Serialization-friendly claim
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    protected record ClaimRecord(string type, object value);
}