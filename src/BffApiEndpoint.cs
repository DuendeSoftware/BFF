using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ReverseProxy.Service.Proxy;

namespace Duende.Bff
{
    public static class BffApiEndpoint
    {
        public static RequestDelegate Map(string localPath, string apiAddress, AccessTokenRequirement accessTokenRequirement)
        {
            return async context =>
            {
                var proxy = context.RequestServices.GetRequiredService<IHttpProxy>();
                var clientFactory = context.RequestServices.GetRequiredService<IDefaultHttpMessageInvokerFactory>();

                var httpClient = clientFactory.CreateClient(localPath);
                
                var result = await context.AuthenticateAsync();
                if (!result.Succeeded && accessTokenRequirement == AccessTokenRequirement.RequireUserToken)
                {
                    context.Response.StatusCode = 401;
                    return;
                }

                var token = await context.GetUserAccessTokenAsync();
                if (string.IsNullOrWhiteSpace(token) &&
                    accessTokenRequirement == AccessTokenRequirement.RequireUserToken)
                {
                    context.Response.StatusCode = 401;
                    return;
                }
                
                var transformer = new ProxyApiTransformer(token);
                var requestOptions = new RequestProxyOptions { Timeout = TimeSpan.FromSeconds(100) };

                await proxy.ProxyAsync(context, apiAddress, httpClient, requestOptions, transformer);

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