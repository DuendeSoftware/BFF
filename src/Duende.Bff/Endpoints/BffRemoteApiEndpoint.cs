// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Service.Proxy;

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
                var options = context.RequestServices.GetRequiredService<BffOptions>();
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

                if (metadata.RequireAntiForgeryHeader)
                {
                    var antiForgeryHeader = context.Request.Headers[options.AntiForgeryHeaderName].FirstOrDefault();
                    if (antiForgeryHeader == null || antiForgeryHeader != options.AntiForgeryHeaderValue)
                    {
                        logger.AntiForgeryValidationFailed(localPath);

                        context.Response.StatusCode = 401;
                        return;
                    }
                }

                string token = null;
                if (metadata.RequiredTokenType.HasValue)
                {
                    switch (metadata.RequiredTokenType)
                    {
                        case TokenType.Client:
                        {
                            token = await context.GetClientAccessTokenAsync();
                            if (string.IsNullOrWhiteSpace(token))
                            {
                                logger.AccessTokenMissing(localPath, metadata.RequiredTokenType.Value);

                                context.Response.StatusCode = 401;
                                return;
                            }

                            break;
                        }
                        case TokenType.User:
                        {
                            token = await context.GetUserAccessTokenAsync();
                            if (string.IsNullOrWhiteSpace(token))
                            {
                                logger.AccessTokenMissing(localPath, metadata.RequiredTokenType.Value);

                                context.Response.StatusCode = 401;
                                return;
                            }

                            break;
                        }
                        case TokenType.UserOrClient:
                        {
                            token = await context.GetUserAccessTokenAsync();
                            if (string.IsNullOrWhiteSpace(token))
                            {
                                token = await context.GetClientAccessTokenAsync();
                                if (string.IsNullOrWhiteSpace(token))
                                {
                                    logger.AccessTokenMissing(localPath, metadata.RequiredTokenType.Value);

                                    context.Response.StatusCode = 401;
                                    return;
                                }
                            }

                            break;
                        }
                    }
                }

                if (token == null)
                {
                    if (metadata.OptionalUserToken)
                    {
                        token = await context.GetUserAccessTokenAsync();
                    }
                }

                var proxy = context.RequestServices.GetRequiredService<IHttpProxy>();
                var clientFactory = context.RequestServices.GetRequiredService<IHttpMessageInvokerFactory>();
                var transformerFactory = context.RequestServices.GetRequiredService<IHttpTransformerFactory>();
                
                var httpClient = clientFactory.CreateClient(localPath);
                var transformer = transformerFactory.CreateTransformer(localPath, token);

                await proxy.ProxyAsync(context, apiAddress, httpClient, new RequestProxyOptions(), transformer);

                var errorFeature = context.Features.Get<IProxyErrorFeature>();
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