// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Duende.Bff;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using IdentityModel.Client;
using System;
using System.Linq;

namespace Host6;

public class AudienceConstrainedAccessTokenRetriever : DefaultAccessTokenRetriever
{
    private const string AudienceConstrainedTokenName = "access_token::audience:isolated-api";

    public AudienceConstrainedAccessTokenRetriever(ILogger<AudienceConstrainedAccessTokenRetriever> logger) : base(logger)
    {
    }
    
    public override async Task<AccessTokenResult> GetAccessToken(AccessTokenRetrievalContext context)
    {
        var audienceToken = await context.HttpContext.GetTokenAsync(AudienceConstrainedTokenName);
        if(audienceToken != null)
        {
            return new BearerTokenResult(audienceToken);
        } 
        else
        {

            var refreshToken = await context.HttpContext.GetTokenAsync("refresh_token");

            if(refreshToken != null)
            {
                var client = new HttpClient();
                var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
                {
                    Address = "https://localhost:5001/connect/token", // TODO - Discovery 

                    ClientId = "spa",
                    ClientSecret = "secret",

                    RefreshToken = refreshToken,

                    Resource = { "urn:isolated-api" }
                });

                if(!response.IsError && response.AccessToken != null)
                {
                    var authResult = await context.HttpContext.AuthenticateAsync() ?? 
                        throw new InvalidOperationException("Authentication failed");
                    var existingTokens = authResult.Properties!.GetTokens();
                    authResult.Properties!.StoreTokens(existingTokens.Append(new AuthenticationToken
                    {
                        Name = AudienceConstrainedTokenName,
                        Value = response.AccessToken
                    }));

                    await context.HttpContext.SignInAsync(authResult.Principal!, authResult.Properties);

                    return new BearerTokenResult(response.AccessToken);
                }
            }
        }

        return new AccessTokenRetrievalError("Failed to obtain an audience constrained access token");
    }
}
