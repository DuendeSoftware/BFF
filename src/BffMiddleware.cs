using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.ReverseProxy.Service.Proxy;

namespace Duende.Bff
{
    public partial class BffMiddleware
    {
        private readonly RequestDelegate _next;
        private BffOptions _options;
        private IHttpProxy _proxy;
        private readonly IAuthenticationSchemeProvider _schemes;
        private HttpMessageInvoker _httpClient;

        public BffMiddleware(RequestDelegate next, IOptions<BffOptions> options, IHttpProxy proxy, IAuthenticationSchemeProvider schemes)
        {
            _next = next;
            _options = options.Value;
            _proxy = proxy;
            _schemes = schemes;

            _httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false
            });
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(_options.PathBase))
            {
                await InvokeApp(context);
            }
            else
            {
                await _next(context);
            }
        }

        private async Task InvokeApp(HttpContext context)
        {
            var path = context.Request.Path.Value.Substring(_options.PathBase.Length);

            if (path.Equals("/login", StringComparison.OrdinalIgnoreCase))
            {
                await InvokeLogin(context);
            }
            else if (path.Equals("/login-callback", StringComparison.OrdinalIgnoreCase))
            {
                await InvokeLoginCallback(context);
            }
            else if (path.Equals("/logout", StringComparison.OrdinalIgnoreCase))
            {
                await InvokeLogout(context);
            }
            else if (path.Equals("/logout-callback", StringComparison.OrdinalIgnoreCase))
            {
                await InvokeLogoutCallback(context);
            }
            else if (path.Equals("/user", StringComparison.OrdinalIgnoreCase))
            {
                await InvokeUser(context);
            }
            else if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                await InvokeApi(context);
            }
            else
            {
                await _next(context);
            }
        }

        private async Task InvokeLogin(HttpContext context)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = $"{_options.PathBase}/login-callback"
            };

            await context.ChallengeAsync(props);
        }

        private async Task InvokeLoginCallback(HttpContext context)
        {
            context.Response.Redirect("/");
        }


        private async Task InvokeLogout(HttpContext context)
        {
            var signInScheme = await _schemes.GetDefaultSignInSchemeAsync();
            await context.SignOutAsync(signInScheme.Name);

            var props = new AuthenticationProperties
            {
                RedirectUri = $"{_options.PathBase}/logout-callback"
            };

            await context.SignOutAsync(props);
        }

        private Task InvokeLogoutCallback(HttpContext context)
        {
            context.Response.Redirect("/");
            return Task.CompletedTask;
        }

        private async Task InvokeUser(HttpContext context)
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
        }

        private async Task InvokeApi(HttpContext context)
        {
            var result = await context.AuthenticateAsync();
            if (!result.Succeeded)
            {
                context.Response.StatusCode = 401;
            }

            var transformer = new ProxyApiTransformer(context);
            var requestOptions = new RequestProxyOptions(TimeSpan.FromSeconds(100), null);

            await _proxy.ProxyAsync(context, "https://localhost:5006", _httpClient, requestOptions, transformer);

            var errorFeature = context.Features.Get<IProxyErrorFeature>();
            if (errorFeature != null)
            {
                var error = errorFeature.Error;
                var exception = errorFeature.Exception;
            }
        }
    }
}