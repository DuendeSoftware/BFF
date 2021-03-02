using Duende.Bff;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class BffServiceCollectionExtensions
    {
        public static IServiceCollection AddBff(this IServiceCollection services)
        {
            services.AddReverseProxy().LoadFromMemory();
            services.AddAccessTokenManagement();

            services.AddSingleton<IDefaultHttpMessageInvokerFactory, DefaultHttpMessageInvokerFactory>();
            services.AddTransient<IAuthorizationMiddlewareResultHandler, BffAuthorizationMiddlewareResultHandler>();

            services.AddSingleton<IUserSessionStore, InMemoryTicketStore>();
            services.AddTransient<IBackchannelLogoutService, BackchannelLogoutService>();

            return services;
        }

        public static IServiceCollection AddCookieTicketStore(this IServiceCollection services)
        {
            services.AddTransient<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureApplicationCookie>();
            services.AddTransient<ITicketStore, CookieTicketStore>();

            return services;
        }

        public static IServiceCollection AddCookieTicketStore<TUserSessionStore>(this IServiceCollection services)
            where TUserSessionStore : class, IUserSessionStore
        {
            services.AddCookieTicketStore();
            services.AddTransient<IUserSessionStore, TUserSessionStore>();

            return services;
        }
    }
}