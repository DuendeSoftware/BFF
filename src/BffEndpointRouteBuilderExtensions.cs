using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Duende.Bff;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
                var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault();

                
                if (!IsLocalUrl(returnUrl))
                {
                    throw new Exception("returnUrl is not application local");
                }

                var props = new AuthenticationProperties
                {
                    RedirectUri = returnUrl ?? "/"
                };

                await context.ChallengeAsync(props);
            });
           
            endpoints.MapGet(basePath +"/logout", async context =>
            {
                var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                
                // get rid of local cookie first
                var signInScheme = await schemes.GetDefaultSignInSchemeAsync();
                await context.SignOutAsync(signInScheme.Name);

                var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault();

                if (!IsLocalUrl(returnUrl))
                {
                    throw new Exception("returnUrl is not application local");
                }

                var props = new AuthenticationProperties
                {
                    RedirectUri = returnUrl ?? "/"
                };

                // trigger idp logout
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

        public static IEndpointConventionBuilder MapBffApiEndpoint(
            this IEndpointRouteBuilder endpoints,
            string localPath, 
            string apiAddress, 
            AccessTokenRequirement accessTokenRequirement)
        {
            return endpoints.Map(localPath + "/{**catch-all}", async context =>
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
            });
        }
        
        
        private static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            // Allows "/" or "/foo" but not "//" or "/\".
            if (url[0] == '/')
            {
                // url is exactly "/"
                if (url.Length == 1)
                {
                    return true;
                }

                // url doesn't start with "//" or "/\"
                if (url[1] != '/' && url[1] != '\\')
                {
                    return !HasControlCharacter(url.AsSpan(1));
                }

                return false;
            }

            // Allows "~/" or "~/foo" but not "~//" or "~/\".
            if (url[0] == '~' && url.Length > 1 && url[1] == '/')
            {
                // url is exactly "~/"
                if (url.Length == 2)
                {
                    return true;
                }

                // url doesn't start with "~//" or "~/\"
                if (url[2] != '/' && url[2] != '\\')
                {
                    return !HasControlCharacter(url.AsSpan(2));
                }

                return false;
            }

            return false;

            static bool HasControlCharacter(ReadOnlySpan<char> readOnlySpan)
            {
                // URLs may not contain ASCII control characters.
                for (var i = 0; i < readOnlySpan.Length; i++)
                {
                    if (char.IsControl(readOnlySpan[i]))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}