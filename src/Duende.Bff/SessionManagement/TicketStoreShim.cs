// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Duende.Bff
{
    /// <summary>
    /// this shim class is needed since ITicketStore is not configured in DI, rather it's a property 
    /// of the cookie options and coordinated with PostConfigureApplicationCookie. #lame
    /// https://github.com/aspnet/AspNetCore/issues/6946 
    /// </summary>
    public class TicketStoreShim : ITicketStore
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BffOptions _options;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        public TicketStoreShim(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<BffOptions>();
        }

        /// <summary>
        /// The inner
        /// </summary>
        private IServerTicketStore Inner => _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IServerTicketStore>();

        /// <inheritdoc />
        public Task RemoveAsync(string key)
        {
            return Inner.RemoveAsync(key);
        }

        /// <inheritdoc />
        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            return Inner.RenewAsync(key, ticket);
        }

        /// <inheritdoc />
        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            var ticket = await Inner.RetrieveAsync(key);

            // allows the client-side app to request that the cookie does not slide on the user endpoint
            // this only works if we're implementing the a ticket store, as we can suppress the behavior
            // by explicitly setting the AllowRefresh on the ticket
            if (ticket != null && _httpContextAccessor.HttpContext.Request.Path == _options.UserPath)
            {
                var slide = _httpContextAccessor.HttpContext.Request.Query[Constants.RequestParameters.SlideCookie];
                if (slide == "false")
                {
                    ticket.Properties.AllowRefresh = false;
                }
            }

            return ticket;
        }

        /// <inheritdoc />
        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            return Inner.StoreAsync(ticket);
        }
    }
}