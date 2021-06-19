// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Net.Http.Headers;
using Duende.Bff;
using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Duende.Bff
{
    /// <summary>
    /// Transform provider to attach an access token to forwarded calls
    /// </summary>
    public class AccessTokenTransformProvider : ITransformProvider
    {
        private readonly BffOptions _options;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options"></param>
        public AccessTokenTransformProvider(BffOptions options)
        {
            _options = options;
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
            string value = null;
            
            // todo: what to do with conflicting values?
            if ((transformBuildContext.Route.Metadata?.TryGetValue(Constants.Yarp.TokenTypeMetadata, out value) ?? false)
                || (transformBuildContext.Cluster?.Metadata?.TryGetValue(Constants.Yarp.TokenTypeMetadata, out value) ?? false))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("A non-empty Duende.Bff.Yarp.TokenType metadata value is required");
                }

                if (!TokenType.TryParse(value, true, out TokenType tokenType))
                {
                    throw new ArgumentException("Invalid value for Duende.Bff.Yarp.TokenType metadata");
                }

                transformBuildContext.AddRequestTransform(async transformContext =>
                {
                    transformContext.HttpContext.CheckForBffMiddleware(_options);
                    
                    var token = await transformContext.HttpContext.GetManagedAccessToken(tokenType);
                    
                    // todo
                    // logger.AccessTokenMissing(localPath, metadata.RequiredTokenType.Value);
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        transformContext.ProxyRequest.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);
                    }
                });
            }
        }
    }
}