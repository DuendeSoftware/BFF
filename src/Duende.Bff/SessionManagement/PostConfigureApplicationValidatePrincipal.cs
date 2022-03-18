// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
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
        private readonly string? _scheme;
        private readonly ILogger<PostConfigureApplicationValidatePrincipal> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="bffOptions"></param>
        /// <param name="authOptions"></param>
        /// <param name="logger"></param>
        public PostConfigureApplicationValidatePrincipal(BffOptions bffOptions, IOptions<AuthenticationOptions> authOptions, ILogger<PostConfigureApplicationValidatePrincipal> logger)
        {
            _options = bffOptions;
            _scheme = authOptions.Value.DefaultAuthenticateScheme ?? authOptions.Value.DefaultScheme;
            _logger = logger;
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
                        _logger.LogDebug("Explicitly setting ShouldRenew=false in OnValidatePrincipal due to query param suppressing slide behavior.");
                        ctx.ShouldRenew = false;

#if !NET6_0_OR_GREATER
                        if (ctx.HttpContext.Items.ContainsKey("Bff-AuthenticationTicket-AllowRefresh"))
                        {
                            // in ASP.NET Core 3.1 we need to track that we have set ticket.Properties.AllowRefresh
                            // so that we can un-set it, in the rare scenario where during this request someone else
                            // downstream re-issues the cookie w/ SignInAsync, because the AllowRefresh that we set
                            // will be cached, and then set internally in the new cookie, and thus it will never slide again.
                            ctx.Properties.AllowRefresh = (bool?)ctx.HttpContext.Items["Bff-AuthenticationTicket-AllowRefresh"];
                        }
#endif
                    }
                }

                return result;
            };

            return Callback;
        }
    }
}
