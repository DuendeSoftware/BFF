// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Duende.Bff
{
    /// <summary>
    /// Cookie configuration for the user session plumbing
    /// </summary>
    public class PostConfigureApplicationCookieSessionStore : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _scheme;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="scheme"></param>
        public PostConfigureApplicationCookieSessionStore(IHttpContextAccessor httpContextAccessor, string scheme = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _scheme = scheme;
        }

        /// <inheritdoc />
        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            if (_scheme == null || name == _scheme)
            {
                options.SessionStore = new TicketStoreShim(_httpContextAccessor);
            }
        }
    }
}