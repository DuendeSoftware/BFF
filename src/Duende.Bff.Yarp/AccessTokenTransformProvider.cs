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
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDPoPProofService _dPoPProofService;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="options"></param>
    /// <param name="loggerFactory"></param>
    /// <param name="dPoPProofService"></param>
    public AccessTokenTransformProvider(IOptions<BffOptions> options, ILoggerFactory loggerFactory, IDPoPProofService dPoPProofService)
    {
        _options = options.Value;
        _loggerFactory = loggerFactory;
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

            var accessTokenTransform = new AccessTokenRequestTransform(
                _dPoPProofService,
                _loggerFactory.CreateLogger<AccessTokenRequestTransform>(),
                token,
                transformBuildContext?.Route?.RouteId, tokenType);

            await accessTokenTransform.ApplyAsync(transformContext);
        });
    }
}