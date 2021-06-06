// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Net.Http.Headers;
using Duende.Bff;
using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace YarpHost
{
    internal class AccessTokenTransformProvider : ITransformProvider
    {
        private readonly BffOptions _options;

        public AccessTokenTransformProvider(BffOptions options)
        {
            _options = options;
        }
        
        public void ValidateRoute(TransformRouteValidationContext context)
        {
        }

        public void ValidateCluster(TransformClusterValidationContext context)
        {
        }

        public void Apply(TransformBuilderContext transformBuildContext)
        {
            string value = null;
            
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