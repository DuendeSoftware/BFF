// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Duende.Bff.Logging;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Duende.Bff;

/// <summary>
/// Default implementation of IAccessTokenRetriever
/// </summary>
public class DefaultAccessTokenRetriever : IAccessTokenRetriever
{
    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly ILogger<DefaultAccessTokenRetriever> Logger;

    /// <summary>
    /// Creates new instances of DefaultAccessTokenRetriever. 
    /// </summary>
    /// <param name="logger"></param>
    public DefaultAccessTokenRetriever(ILogger<DefaultAccessTokenRetriever> logger)
    {
        Logger = logger;
    }

    /// <inheritdoc />
    public virtual async Task<AccessTokenResult> GetAccessToken(AccessTokenRetrievalContext context)
    {
        AccessTokenResult? token = null;
        if (context.Metadata.RequiredTokenType.HasValue)
        {
            var tokenType = context.Metadata.RequiredTokenType.Value;
            token = await context.HttpContext.GetManagedAccessToken(tokenType, optional: true, context.UserTokenRequestParameters);
            if (token == null)
            {
                Logger.AccessTokenMissing(context.LocalPath, tokenType.ToString());
                return new AccessTokenRetrievalError("Access token not found");
            }
        }

        if (token == null)
        {
            if (context.Metadata.OptionalUserToken)
            {
                var userAccessToken = await context.HttpContext.GetUserAccessTokenAsync(context.UserTokenRequestParameters);
                
                // TODO - This is more copy-pasta
                return userAccessToken switch
                {
                    null or { AccessToken: null } => new NoAccessTokenResult(),
                    { AccessTokenType: OidcConstants.TokenResponse.BearerTokenType } => 
                        new BearerTokenResult(userAccessToken.AccessToken),
                    { AccessTokenType: OidcConstants.TokenResponse.DPoPTokenType, DPoPJsonWebKey: not null } =>
                        new DPoPTokenResult(userAccessToken.AccessToken, userAccessToken.DPoPJsonWebKey),
                    { AccessTokenType: string accessTokenType } => 
                        new AccessTokenRetrievalError($"Unexpected AccessTokenType: {accessTokenType}"),
                    { AccessTokenType: null } =>
                        new AccessTokenRetrievalError($"Missing AccessTokenType")
                };
            }
        }
        return token;
    }
}
