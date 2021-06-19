// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Forwarder;

namespace Duende.Bff
{
    /// <summary>
    /// Remote BFF API endpoint
    /// </summary>
    public static class BffRemoteApiEndpoint
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
                    throw new InvalidOperationException("endoint not found");
                }

                var metadata = endpoint.Metadata.GetMetadata<BffRemoteApiEndpointMetadata>();
                if (metadata == null)
                {
                    throw new InvalidOperationException("API endoint is missing BFF metadata");
                }

                string token = null;
                if (metadata.RequiredTokenType.HasValue)
                {
                    token = await context.GetManagedAccessToken(metadata.RequiredTokenType.Value);
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
                        token = await context.GetUserAccessTokenAsync();
                    }
                }

                var proxy = context.RequestServices.GetRequiredService<IHttpForwarder>();
                var clientFactory = context.RequestServices.GetRequiredService<IHttpMessageInvokerFactory>();
                var transformerFactory = context.RequestServices.GetRequiredService<IHttpTransformerFactory>();
                
                var httpClient = clientFactory.CreateClient(localPath);
                var transformer = transformerFactory.CreateTransformer(localPath, token);

                await proxy.SendAsync(context, apiAddress, httpClient, ForwarderRequestConfig.Empty, transformer);

                // todo: check if return value hanlding is better
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