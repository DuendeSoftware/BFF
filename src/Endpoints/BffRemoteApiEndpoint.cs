using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ReverseProxy.Service.Proxy;

namespace Duende.Bff
{
    public static class BffRemoteApiEndpoint
    {
        public static RequestDelegate Map(string localPath, string apiAddress)
        {
            return async context =>
            {
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Duende.Bff.BffApiEndpoint");

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
                    var antiForgeryHeader = context.Request.Headers["X-CSRF"].FirstOrDefault();
                    if (antiForgeryHeader == null || antiForgeryHeader != "1")
                    {
                        logger.AntiForgeryValidationFailed(localPath);

                        context.Response.StatusCode = 401;
                        return;
                    }
                }

                string token = null;
                if (metadata.RequiredTokenType.HasValue)
                {
                    if (metadata.RequiredTokenType == TokenType.Client)
                    {
                        token = await context.GetClientAccessTokenAsync();
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            logger.AccessTokenMissing(localPath, metadata.RequiredTokenType.Value);

                            context.Response.StatusCode = 401;
                            return;
                        }
                    }
                    else if (metadata.RequiredTokenType == TokenType.User)
                    {
                        token = await context.GetUserAccessTokenAsync();
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            logger.AccessTokenMissing(localPath, metadata.RequiredTokenType.Value);

                            context.Response.StatusCode = 401;
                            return;
                        }
                    }
                    else if (metadata.RequiredTokenType == TokenType.UserOrClient)
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
                var httpClient = clientFactory.CreateClient(localPath);

                var transformer = new BffHttpTransformer(token, context.Request.Path, new PathString(localPath),
                    context.Request.QueryString);
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