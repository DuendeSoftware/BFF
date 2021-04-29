// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Duende.Bff
{
    /// <summary>
    /// Cookie configuration to suppress sliding the cookie on the ~/bff/user endpoint if requested.
    /// </summary>
    public class PostConfigureApplicationValidatePrincipal : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly BffOptions _options;
        private readonly string _scheme;
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="bffOptions"></param>
        /// <param name="authOptions"></param>
        public PostConfigureApplicationValidatePrincipal(BffOptions bffOptions, IOptions<AuthenticationOptions> authOptions)
        {
            _options = bffOptions;
            _scheme = authOptions.Value.DefaultAuthenticateScheme ?? authOptions.Value.DefaultScheme;
        }

        /// <inheritdoc />
        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            if (name == _scheme)
            {
                options.Events.OnValidatePrincipal = CreateCallback(options.Events.OnValidatePrincipal);
            }
        }

        private Func<CookieValidatePrincipalContext, Task> CreateCallback(Func<CookieValidatePrincipalContext, Task> inner)
        {
            Task Callback(CookieValidatePrincipalContext ctx)
            {
                var result = inner?.Invoke(ctx) ?? Task.CompletedTask;

                // allows the client-side app to request that the cookie does not slide on the user endpoint
                // we must add this logic in the OnValidatePrincipal because it's a code path that can trigger the 
                // cookie to slide regardless of the CookieOption's sliding feature
                // we suppress the behavior by setting ShouldRenew on the validate principal context
                if (ctx.HttpContext.Request.Path == _options.UserPath)
                {
                    var slide = ctx.Request.Query[Constants.RequestParameters.SlideCookie];
                    if (slide == "false")
                    {
                        ctx.ShouldRenew = false;
                    }
                }

                return result;
            };

            return Callback;
        }
    }
}
