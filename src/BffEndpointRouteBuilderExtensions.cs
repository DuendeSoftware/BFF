using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Duende.Bff;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ReverseProxy.Service.Proxy;

namespace Microsoft.AspNetCore.Builder
{
    public static class BffEndpointRouteBuilderExtensions
    {
        public static void MapBffSessionEndpoints(
            this IEndpointRouteBuilder endpoints,
            string basePath)
        {
            endpoints.MapGet(basePath + "/login", async context =>
            {
                var props = new AuthenticationProperties
                {
                    RedirectUri = $"{basePath}/login-callback"
                };

                await context.ChallengeAsync(props);
            });
            
            
            endpoints.MapGet(basePath +"/login-callback", async context =>
            {
                context.Response.Redirect("/");
            });
            
            endpoints.MapGet(basePath +"/logout", async context =>
            {
                var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                
                var signInScheme = await schemes.GetDefaultSignInSchemeAsync();
                await context.SignOutAsync(signInScheme.Name);

                var props = new AuthenticationProperties
                {
                    RedirectUri = $"{basePath}/logout-callback"
                };

                await context.SignOutAsync(props);
            });
            
            endpoints.MapGet(basePath +"/logout-callback", async context =>
            {
                context.Response.Redirect("/");
            });
            
            endpoints.MapGet(basePath +"/user", async context =>
            {
                var result = await context.AuthenticateAsync();

                if (!result.Succeeded)
                {
                    context.Response.StatusCode = 401;
                }
                else
                {
                    var claims = result.Principal.Claims.Select(x => new { x.Type, x.Value });
                    var json = JsonSerializer.Serialize(claims);

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(json, Encoding.UTF8);
                }
            });
        }

        public static void MapBffApiEndpoint(
            this IEndpointRouteBuilder endpoints,
            string localPath, 
            string apiAddress, 
            AccessTokenRequirement accessTokenRequirement)
        {
            endpoints.Map(localPath + "/{**catch-all}", async context =>
            {
                var proxy = context.RequestServices.GetRequiredService<IHttpProxy>();
                var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
                {
                    UseProxy = false,
                    AllowAutoRedirect = false,
                    AutomaticDecompression = DecompressionMethods.None,
                    UseCookies = false
                });
                
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
                var requestOptions = new RequestProxyOptions(TimeSpan.FromSeconds(100), null);

                await proxy.ProxyAsync(context, apiAddress, httpClient, requestOptions, transformer);

                var errorFeature = context.Features.Get<IProxyErrorFeature>();
                if (errorFeature != null)
                {
                    var error = errorFeature.Error;
                    var exception = errorFeature.Exception;
                }
            });
            
            
        }
        
    }
}