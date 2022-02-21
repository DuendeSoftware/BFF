// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Duende.Bff.Logging;
using Duende.Bff.Yarp.Logging;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Forwarder;

namespace Duende.Bff.Yarp
{
    /// <summary>
    /// Remote BFF API endpoint
    /// </summary>
    public static class RemoteApiEndpoint
    {
        /// <summary>
        /// Endpoint logic
        /// </summary>
        /// <param name="localPath">The local path (e.g. /api)</param>
        /// <param name="apiAddress">The remote address (e.g. https://api.myapp.com/foo)</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static RequestDelegate Map(string localPath, string apiAddress)
        {
            return async context =>
            {
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(LogCategories.RemoteApiEndpoints);

                var endpoint = context.GetEndpoint();
                if (endpoint == null)
                {
                    throw new InvalidOperationException("endpoint not found");
                }

                var metadata = endpoint.Metadata.GetMetadata<BffRemoteApiEndpointMetadata>();
                if (metadata == null)
                {
                    throw new InvalidOperationException("API endoint is missing BFF metadata");
                }

                UserAccessTokenParameters? toUserAccessTokenParameters = null;

                if (metadata.BffUserAccessTokenParameters != null)
                {
                    toUserAccessTokenParameters = metadata.BffUserAccessTokenParameters.ToUserAccessTokenParameters();
                }

                string? token = null;
                if (metadata.RequiredTokenType.HasValue)
                {
                    token = await context.GetManagedAccessToken(metadata.RequiredTokenType.Value, toUserAccessTokenParameters);
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        logger.AccessTokenMissing(localPath, metadata.RequiredTokenType.Value);

                        context.Response.StatusCode = 401;
                        return;
                    }
                }

                if (token == null)
                {
                    if (metadata.OptionalUserToken)
                    {
                        token = await context.GetUserAccessTokenAsync(toUserAccessTokenParameters);
                    }
                }

                var forwarder = context.RequestServices.GetRequiredService<IHttpForwarder>();
                var clientFactory = context.RequestServices.GetRequiredService<IHttpMessageInvokerFactory>();
                var transformerFactory = context.RequestServices.GetRequiredService<IHttpTransformerFactory>();
                
                var httpClient = clientFactory.CreateClient(localPath);
                var transformer = transformerFactory.CreateTransformer(localPath, token);

                await forwarder.SendAsync(context, apiAddress, httpClient, ForwarderRequestConfig.Empty, transformer);

                var errorFeature = context.Features.Get<IForwarderErrorFeature>();
                if (errorFeature != null)
                {
                    var error = errorFeature.Error;
                    var exception = errorFeature.Exception;

                    logger.ProxyResponseError(localPath, exception?.ToString() ?? error.ToString());
                }
            };
        }
    }
}