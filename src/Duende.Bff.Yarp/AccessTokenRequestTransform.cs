// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Duende.AccessTokenManagement;
using IdentityModel;
using Yarp.ReverseProxy.Transforms;

namespace Duende.Bff.Yarp;

/// <summary>
/// Adds an access token to outgoing requests
/// </summary>
public class AccessTokenRequestTransform : RequestTransform
{
    private readonly AccessTokenResult _token;
    private readonly IDPoPProofService _dPoPProofService;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="accessToken"></param>
    public AccessTokenRequestTransform(AccessTokenResult accessToken, IDPoPProofService dPoPProofService)
    {
        _token = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        this._dPoPProofService = dPoPProofService;
    }
        
    /// <inheritdoc />
    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        // TODO - This logic is almost identical to the AccessTokenTransformProvider
        if (_token != null)
        {
            if (_token is BearerAccessToken bearerToken)
            {
                ApplyBearerToken(context, bearerToken);
            }
            else if (_token is DPoPAccessToken dpopToken)
            {
                await ApplyDPoPToken(context, dpopToken);
            }
            else
            {
                // TODO - log a warning that the token type is weird
            }
        }
    }

    private void ApplyBearerToken(RequestTransformContext context, BearerAccessToken token)
    {
        context.ProxyRequest.Headers.Authorization = 
            new AuthenticationHeaderValue(OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer, token.AccessToken);
    }

    private async Task ApplyDPoPToken(RequestTransformContext context, DPoPAccessToken token)
    {
        ArgumentNullException.ThrowIfNull(token.DPoPJsonWebKey, nameof(token.DPoPJsonWebKey));

        var proofToken = await _dPoPProofService.CreateProofTokenAsync(new DPoPProofRequest
        {
            AccessToken = token.AccessToken,
            DPoPJsonWebKey = token.DPoPJsonWebKey,
            Method = context.ProxyRequest.Method.ToString(),
            Url = context.ProxyRequest.GetDPoPUrl()
        });
        if(proofToken != null)
        {
            context.ProxyRequest.Headers.Add(OidcConstants.HttpHeaders.DPoP, proofToken.ProofToken);
            context.ProxyRequest.Headers.Authorization = 
                new AuthenticationHeaderValue(OidcConstants.AuthenticationSchemes.AuthorizationHeaderDPoP, token.AccessToken);
        }
    }
}