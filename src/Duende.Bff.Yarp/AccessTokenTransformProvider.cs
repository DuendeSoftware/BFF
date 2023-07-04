// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Duende.AccessTokenManagement;
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

    public bool GetMetadataValue(TransformBuilderContext transformBuildContext, string metadataName, [NotNullWhen(true)] out string? metadata)
    {
        var routeValue = transformBuildContext.Route.Metadata?.GetValueOrDefault(metadataName);
        var clusterValue =
            transformBuildContext.Cluster?.Metadata?.GetValueOrDefault(metadataName);

        // no metadata
        if (string.IsNullOrEmpty(routeValue) && string.IsNullOrEmpty(clusterValue))
        {
            metadata = default;
            return false;
        }

        var values = new HashSet<string>();
        if (!string.IsNullOrEmpty(routeValue)) values.Add(routeValue);
        if (!string.IsNullOrEmpty(clusterValue)) values.Add(clusterValue);

        if (values.Count > 1)
        {
            throw new ArgumentException(
                $"Mismatching {metadataName} route and cluster metadata values found");
        }
        metadata = values.First();
        return true;
    }

    /// <inheritdoc />
    public void Apply(TransformBuilderContext transformBuildContext)
    {
        TokenType tokenType;
        bool optional;
        if(GetMetadataValue(transformBuildContext, Constants.Yarp.OptionalUserTokenMetadata, out var optionalTokenMetadata))
        {
            optional = true;
            tokenType = TokenType.User;
            // TODO - is it an error to set both OptionalUserToken and a token type? I think yes, because setting a token type means
            // setting a *required* token type.
        } 
        else if (GetMetadataValue(transformBuildContext, Constants.Yarp.TokenTypeMetadata, out var tokenTypeMetadata))
        {
            optional = false;
            if (!TokenType.TryParse(tokenTypeMetadata, true, out tokenType))
            {
                throw new ArgumentException("Invalid value for Duende.Bff.Yarp.TokenType metadata");
            }
        }
        else
        {
            return;
        }

        transformBuildContext.AddRequestTransform(async transformContext =>
        {
            transformContext.HttpContext.CheckForBffMiddleware(_options);

            var token = await transformContext.HttpContext.GetManagedAccessToken(tokenType, optional);

            var accessTokenTransform = new AccessTokenRequestTransform(
                _dPoPProofService,
                _loggerFactory.CreateLogger<AccessTokenRequestTransform>(),
                token,
                transformBuildContext?.Route?.RouteId, tokenType);

            await accessTokenTransform.ApplyAsync(transformContext);
        });
    }
}