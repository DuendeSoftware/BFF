// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
namespace Duende.Bff;

/// <summary>
/// Default implementation of IAccessTokenRetriever
/// </summary>
public class DefaultAccessTokenRetriever : IAccessTokenRetriever
{
    /// <inheritdoc />
    public virtual async Task<AccessTokenResult> GetAccessToken(AccessTokenRetrievalContext context)
    {
        string? token = null;
        if (context.Metadata.RequiredTokenType.HasValue)
        {
            var tokenType = context.Metadata.RequiredTokenType.Value;
            token = await context.HttpContext.GetManagedAccessToken(tokenType, context.UserTokenRequestParameters);
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AccessTokenResult
                {
                    IsError = true
                };
            }
        }

        if (token == null)
        {
            if (context.Metadata.OptionalUserToken)
            {
                token = (await context.HttpContext.GetUserAccessTokenAsync(context.UserTokenRequestParameters)).AccessToken;
            }

        }
        return new AccessTokenResult
        {
            Token = token,
            IsError = false
        };
    }
}
