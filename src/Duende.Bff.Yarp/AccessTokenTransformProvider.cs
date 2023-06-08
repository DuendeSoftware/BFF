// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Duende.AccessTokenManagement;
using Duende.Bff.Logging;
using Duende.Bff.Yarp.Logging;
using IdentityModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Duende.Bff.Yarp;

/// <summary>
/// Transform provider to attach an access token to forwarded calls
/// </summary>
public class AccessTokenTransformProvider : ITransformProvider
{
    private readonly BffOptions _options;
    private readonly ILogger _logger;
    private readonly IDPoPProofService _dPoPProofService;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="dPoPProofService"></param>
    public AccessTokenTransformProvider(IOptions<BffOptions> options, ILogger<AccessTokenTransformProvider> logger, IDPoPProofService dPoPProofService)
    {
        _options = options.Value;
        _logger = logger;
        _dPoPProofService = dPoPProofService;
    }

    /// <inheritdoc />
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    /// <inheritdoc />
    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    /// <inheritdoc />
    public void Apply(TransformBuilderContext transformBuildContext)
    {
        var routeValue = transformBuildContext.Route.Metadata?.GetValueOrDefault(Constants.Yarp.TokenTypeMetadata);
        var clusterValue =
            transformBuildContext.Cluster?.Metadata?.GetValueOrDefault(Constants.Yarp.TokenTypeMetadata);

        // no metadata
        if (string.IsNullOrEmpty(routeValue) && string.IsNullOrEmpty(clusterValue))
        {
            return;
        }

        var values = new HashSet<string>();
        if (!string.IsNullOrEmpty(routeValue)) values.Add(routeValue);
        if (!string.IsNullOrEmpty(clusterValue)) values.Add(clusterValue);

        if (values.Count > 1)
        {
            throw new ArgumentException(
                "Mismatching Duende.Bff.Yarp.TokenType route or cluster metadata values found");
        }
            
        if (!TokenType.TryParse(values.First(), true, out TokenType tokenType))
        {
            throw new ArgumentException("Invalid value for Duende.Bff.Yarp.TokenType metadata");
        }

        transformBuildContext.AddRequestTransform(async transformContext =>
        {
            transformContext.HttpContext.CheckForBffMiddleware(_options);

            var token = await transformContext.HttpContext.GetManagedAccessToken(tokenType);

            if (token != null)
            {
                if(token.AccessTokenType == OidcConstants.TokenResponse.BearerTokenType)
                {
                    ApplyBearerToken(transformContext, token);
                }
                else if (token.AccessTokenType == OidcConstants.TokenResponse.DPoPTokenType) 
                {
                    await ApplyDPoPToken(transformContext, token);
                }
                else 
                {
                    // TODO - log a warning that the token type is weird
                }
            }
            else
            {
                // short circuit forwarder and return 401
                transformContext.HttpContext.Response.StatusCode = 401;
                
                _logger.AccessTokenMissing(transformBuildContext?.Route?.RouteId ?? "unknown route", tokenType);
            }
        });
    }

    private void ApplyBearerToken(RequestTransformContext context, ClientCredentialsToken token)
    {
        context.ProxyRequest.Headers.Authorization = 
            new AuthenticationHeaderValue(OidcConstants.AuthenticationSchemes.AuthorizationHeaderBearer, token.AccessToken);
    }

    private async Task ApplyDPoPToken(RequestTransformContext context, ClientCredentialsToken token)
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