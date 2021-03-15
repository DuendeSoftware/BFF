using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Duende.Bff
{
    /// <summary>
    /// Cookie configuration for the user session plumbing
    /// </summary>
    public class PostConfigureBffApplicationCookie : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public PostConfigureBffApplicationCookie(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            options.SessionStore = new TicketStoreShim(_httpContextAccessor);
        }
    }
}