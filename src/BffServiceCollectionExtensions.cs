using Duende.Bff;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

            services.AddTransient<IBackchannelLogoutService, BackchannelLogoutService>();
            services.TryAddTransient<ISessionRevocationService, NopSessionRevocationService>();
            
            return services;
        }

        public static IServiceCollection AddCookieTicketStore(this IServiceCollection services)
        {
            services.AddTransient<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureApplicationCookie>();
            services.AddTransient<ITicketStore, CookieTicketStore>();

            services.TryAddSingleton<IUserSessionStore, InMemoryTicketStore>();
            services.AddTransient<ISessionRevocationService>(x => x.GetRequiredService<IUserSessionStore>());

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