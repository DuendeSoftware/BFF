using System;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ReverseProxy.Service.Proxy;

namespace Duende.Bff
{
    public static class BffApiEndpoint
    {
        public static RequestDelegate Map(string localPath, string apiAddress)
        {
            return async context =>
            {
                var endpoint = context.GetEndpoint();
                if (endpoint == null)
                {
                    throw new InvalidOperationException("endoint not found");
                }
                
                var metadata = endpoint.Metadata.GetMetadata<BffApiEndointMetadata>();
                if (metadata == null)
                {
                    throw new InvalidOperationException("API endoint is missing metadata");
                }
                
                if (metadata.RequireAntiForgeryToken)
                {
                    var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

                    try
                    {
                        await antiforgery.ValidateRequestAsync(context);
                    }
                    catch (Exception e)
                    {
                        // todo: logging
                        
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
                            context.Response.StatusCode = 401;
                            return;
                            
                            // logging
                        }
                    }
                    else if (metadata.RequiredTokenType == TokenType.User)
                    {
                        token = await context.GetUserAccessTokenAsync();
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            context.Response.StatusCode = 401;
                            return;
                            
                            // logging
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
                                context.Response.StatusCode = 401;
                                return;
                                
                                // logging
                            }
                        }
                    }
                }

                if (metadata.OptionalUserToken)
                {
                    token = await context.GetUserAccessTokenAsync();
                }
                
                var proxy = context.RequestServices.GetRequiredService<IHttpProxy>();
                var clientFactory = context.RequestServices.GetRequiredService<IHttpMessageInvokerFactory>();
                var httpClient = clientFactory.CreateClient(localPath);
                
                var transformer = new AccessTokenTransformer(token);
                var requestOptions = new RequestProxyOptions { Timeout = TimeSpan.FromSeconds(100) };

                await proxy.ProxyAsync(context, apiAddress, httpClient, requestOptions, transformer);

                // todo: logging
                var errorFeature = context.Features.Get<IProxyErrorFeature>();
                if (errorFeature != null)
                {
                    var error = errorFeature.Error;
                    var exception = errorFeature.Exception;
                }
            };
        }
    }
}