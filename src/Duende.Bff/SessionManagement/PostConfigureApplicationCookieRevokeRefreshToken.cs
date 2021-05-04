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
    /// Cookie configuration to revoke refresh token on logout.
    /// </summary>
    public class PostConfigureApplicationCookieRevokeRefreshToken : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly BffOptions _options;
        private readonly string _scheme;
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="bffOptions"></param>
        /// <param name="authOptions"></param>
        public PostConfigureApplicationCookieRevokeRefreshToken(BffOptions bffOptions, IOptions<AuthenticationOptions> authOptions)
        {
            _options = bffOptions;
            _scheme = authOptions.Value.DefaultAuthenticateScheme ?? authOptions.Value.DefaultScheme;
        }

        /// <inheritdoc />
        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            if (_options.RevokeRefreshTokenOnLogout && name == _scheme)
            {
                options.Events.OnSigningOut = CreateCallback(options.Events.OnSigningOut);
            }
        }

        private Func<CookieSigningOutContext, Task> CreateCallback(Func<CookieSigningOutContext, Task> inner)
        {
            async Task Callback(CookieSigningOutContext ctx)
            {
                await ctx.HttpContext.RevokeUserRefreshTokenAsync();
                await inner?.Invoke(ctx);
            };

            return Callback;
        }
    }
}
