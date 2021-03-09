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
            

            services.AddTransient<IBackchannelLogoutService, DefaultBackchannelLogoutService>();
            services.TryAddTransient<ISessionRevocationService, NopSessionRevocationService>();
            
            #if NET5_0
            services.AddTransient<IAuthorizationMiddlewareResultHandler, BffAuthorizationMiddlewareResultHandler>();
            #endif
            
            return services;
        }

        public static IServiceCollection AddCookieTicketStore(this IServiceCollection services)
        {
            services.AddTransient<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureBffApplicationCookie>();
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