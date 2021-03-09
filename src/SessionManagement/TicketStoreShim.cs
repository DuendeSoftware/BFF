using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Duende.Bff
{
    // this shim class is needed since ITicketStore is not configured in DI, rather it's a property 
    // of the cookie options and coordinated with PostConfigureApplicationCookie. #lame
    // https://github.com/aspnet/AspNetCore/issues/6946
    public class TicketStoreShim : ITicketStore
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TicketStoreShim(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ITicketStore Inner
        {
            get
            {
                return _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ITicketStore>();
            }
        }

        public Task RemoveAsync(string key)
        {
            return Inner.RemoveAsync(key);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            return Inner.RenewAsync(key, ticket);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            return Inner.RetrieveAsync(key);
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            return Inner.StoreAsync(ticket);
        }
    }
}