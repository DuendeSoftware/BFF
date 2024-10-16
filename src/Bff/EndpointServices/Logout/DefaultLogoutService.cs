﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.AccessTokenManagement.OpenIdConnect;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Duende.Bff;

/// <summary>
/// Service for handling logout requests
/// </summary>
public class DefaultLogoutService : ILogoutService
{
    /// <summary>
    /// The BFF options
    /// </summary>
    protected readonly BffOptions Options;
        
    /// <summary>
    /// The scheme provider
    /// </summary>
    protected readonly IAuthenticationSchemeProvider AuthenticationSchemeProvider;

    /// <summary>
    /// The return URL validator
    /// </summary>
    protected readonly IReturnUrlValidator ReturnUrlValidator;

    /// <summary>
    /// Service to interact with the token endpoint.
    /// </summary>
    protected readonly IUserTokenEndpointService TokenEndpoint;

    /// <summary>
    /// The logger
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Ctor
    /// </summary>
    public DefaultLogoutService(IOptions<BffOptions> options, 
        IAuthenticationSchemeProvider authenticationAuthenticationSchemeProviderProvider, 
        IReturnUrlValidator returnUrlValidator,
        IUserTokenEndpointService tokenEndpoint,
        ILogger<DefaultLogoutService> logger)
    {
        Options = options.Value;
        AuthenticationSchemeProvider = authenticationAuthenticationSchemeProviderProvider;
        ReturnUrlValidator = returnUrlValidator;
        TokenEndpoint = tokenEndpoint;
        Logger = logger;
    }

    /// <inheritdoc />
    public virtual async Task ProcessRequestAsync(HttpContext context)
    {
        Logger.LogDebug("Processing logout request");

        context.CheckForBffMiddleware(Options);
            
        var result = await context.AuthenticateAsync();
        if (result.Succeeded && result.Principal?.Identity?.IsAuthenticated == true)
        {
            var userSessionId = result.Principal.FindFirst(JwtClaimTypes.SessionId)?.Value;
            if (!String.IsNullOrWhiteSpace(userSessionId))
            {
                var passedSessionId = context.Request.Query[JwtClaimTypes.SessionId].FirstOrDefault();
                // for an authenticated user, if they have a session id claim,
                // we require the logout request to pass that same value to
                // prevent unauthenticated logout requests (similar to OIDC front channel)
                if (Options.RequireLogoutSessionId && userSessionId != passedSessionId)
                {
                    throw new Exception("Invalid Session Id");
                }
            }
        }
            
        var returnUrl = context.Request.Query[Constants.RequestParameters.ReturnUrl].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            if (!await ReturnUrlValidator.IsValidAsync(returnUrl))
            {
                throw new Exception("returnUrl is not valid: " + returnUrl);
            }
        }

        if (Options.RevokeRefreshTokenOnLogout && result.Ticket != null)
        {
            var refreshToken = result.Ticket.Properties.GetTokenValue("refresh_token");
            if (!String.IsNullOrWhiteSpace(refreshToken))
            {
                await TokenEndpoint.RevokeRefreshTokenAsync(new UserToken { RefreshToken = refreshToken }, new UserTokenRequestParameters());

                Logger.LogDebug("Refresh token revoked for sub {sub} and sid {sid}", result.Ticket.GetSubjectId(), result.Ticket.GetSessionId());
            }
            else
            {
                Logger.LogTrace("Refresh token not found for sub {sub} and sid {sid}", result.Ticket.GetSubjectId(), result.Ticket.GetSessionId());
            }
        }

        // get rid of local cookie first
        var signInScheme = await AuthenticationSchemeProvider.GetDefaultSignInSchemeAsync();
        await context.SignOutAsync(signInScheme?.Name);

        if (String.IsNullOrWhiteSpace(returnUrl))
        {
            if (context.Request.PathBase.HasValue)
            {
                returnUrl = context.Request.PathBase;
            }
            else
            {
                returnUrl = "/";
            }
        }
        
        var props = new AuthenticationProperties
        {
            RedirectUri = returnUrl
        };

        Logger.LogDebug("Logout endpoint triggering SignOut with returnUrl {returnUrl}", returnUrl);

        // trigger idp logout
        await context.SignOutAsync(props);
    }
}