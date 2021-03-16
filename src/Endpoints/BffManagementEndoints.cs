using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Duende.Bff
{
    /// <summary>
    /// BFF management endpoints
    /// </summary>
    public static class BffManagementEndoints
    {
        /// <summary>
        /// Login endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task MapLogin(HttpContext context)
        {
            var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                if (!IsLocalUrl(returnUrl))
                {
                    throw new Exception("returnUrl is not application local");
                }
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? "/"
            };

            await context.ChallengeAsync(props);
        }

        /// <summary>
        /// logout endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task MapLogout(HttpContext context)
        {
            var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

            // get rid of local cookie first
            var signInScheme = await schemes.GetDefaultSignInSchemeAsync();
            await context.SignOutAsync(signInScheme.Name);

            var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                if (!IsLocalUrl(returnUrl))
                {
                    throw new Exception("returnUrl is not application local");
                }
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? "/"
            };

            // trigger idp logout
            await context.SignOutAsync(props);
        }

        /// <summary>
        /// user endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task MapUser(HttpContext context)
        {
            var antiForgeryHeader = context.Request.Headers[BffDefaults.AntiForgeryHeaderName].FirstOrDefault();
            if (antiForgeryHeader == null || antiForgeryHeader != BffDefaults.AntiForgeryHeaderValue)
            {
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("Duende.Bff.BffApiEndpoint");
                
                logger.AntiForgeryValidationFailed("user");

                context.Response.StatusCode = 401;
                return;
            }
            
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

        /// <summary>
        /// back-channel logout endpoint
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Task MapBackchannelLogout(HttpContext context)
        {
            var backchannel = context.RequestServices.GetRequiredService<IBackchannelLogoutService>();
            return backchannel.ProcessRequequestAsync(context);
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