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
    private readonly ClientCredentialsToken _token;
    private readonly IDPoPProofService _dPoPProofService;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="dPoPProofService"></param>
    public AccessTokenRequestTransform(ClientCredentialsToken accessToken, IDPoPProofService dPoPProofService)
    {
        _token = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        this._dPoPProofService = dPoPProofService;
    }
        
    /// <inheritdoc />
    public override async ValueTask ApplyAsync(RequestTransformContext context)
    {
        if(_token.AccessTokenType == OidcConstants.TokenResponse.BearerTokenType)
        {
            ApplyBearerToken(context);
        }
        else if (_token.AccessTokenType == OidcConstants.TokenResponse.DPoPTokenType) 
        {
            await ApplyDPoPToken(context);
        }
        else 
        {
            // TODO - log a warning that the token type is weird
        }
    }

    private void ApplyBearerToken(RequestTransformContext context)
    {
        context.ProxyRequest.Headers.Authorization = 
            new AuthenticationHeaderValue(OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer, _token.AccessToken);
    }

    private async Task ApplyDPoPToken(RequestTransformContext context)
    {
        ArgumentNullException.ThrowIfNull(_token.DPoPJsonWebKey, nameof(_token.DPoPJsonWebKey));

        var proofToken = await _dPoPProofService.CreateProofTokenAsync(new DPoPProofRequest
        {
            AccessToken = _token.AccessToken,
            DPoPJsonWebKey = _token.DPoPJsonWebKey,
            Method = context.ProxyRequest.Method.ToString(),
            Url = context.ProxyRequest.GetDPoPUrl()
        });
        if(proofToken != null)
        {
            context.ProxyRequest.Headers.Add(OidcConstants.HttpHeaders.DPoP, proofToken.ProofToken);
            context.ProxyRequest.Headers.Authorization = 
                new AuthenticationHeaderValue(OidcConstants.AuthenticationSchemes.AuthorizationHeaderDPoP, _token.AccessToken);
        }
    }
}