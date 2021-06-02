// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Duende.Bff;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Yarp.ReverseProxy.Abstractions;
using Yarp.ReverseProxy.Abstractions.Config;
using Yarp.ReverseProxy.Service.RuntimeModel.Transforms;

namespace YARP.Sample
{
    internal class AccessTokenTransformProvider : ITransformProvider
    {
        public void ValidateRoute(TransformRouteValidationContext context)
        {
        }

        public void ValidateCluster(TransformClusterValidationContext context)
        {
        }

        public void Apply(TransformBuilderContext transformBuildContext)
        {
            string value = null;
            
            if ((transformBuildContext.Route.Metadata?.TryGetValue("Duende.Bff.TokenType", out value) ?? false)
                || (transformBuildContext.Cluster?.Metadata?.TryGetValue("Duende.Bff.TokenType", out value) ?? false))
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("A non-empty Duende.Bff.TokenType value is required");
                }

                if (!TokenType.TryParse(value, true, out TokenType tokenType))
                {
                    throw new ArgumentException("Invalid value for Duende.Bff.TokenType");
                }

                transformBuildContext.AddRequestTransform(async transformContext =>
                {
                    string token;

                    if (tokenType == TokenType.User)
                    {
                        token = await transformContext.HttpContext.GetUserAccessTokenAsync();
                    }
                    else if (tokenType == TokenType.Client)
                    {
                        token = await transformContext.HttpContext.GetClientAccessTokenAsync();
                    }
                    else
                    {
                        token = await transformContext.HttpContext.GetUserAccessTokenAsync();

                        if (string.IsNullOrEmpty(token))
                        {
                            token = await transformContext.HttpContext.GetClientAccessTokenAsync();
                        }
                    }

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