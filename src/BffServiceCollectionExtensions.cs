using Duende.Bff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the BFF DI services
    /// </summary>
    public static class BffServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Duende.BFF services to DI
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static BffBuilder AddBff(this IServiceCollection services)
        {
            services.AddHttpProxy();
            services.AddAccessTokenManagement();

            services.TryAddSingleton<IHttpMessageInvokerFactory, DefaultHttpMessageInvokerFactory>();
            services.AddTransient<IBackchannelLogoutService, DefaultBackchannelLogoutService>();
            services.TryAddTransient<ISessionRevocationService, NopSessionRevocationService>();
            
            #if NET5_0
            services.AddTransient<IAuthorizationMiddlewareResultHandler, BffAuthorizationMiddlewareResultHandler>();
            #endif
            
            return new BffBuilder(services);
        }
    }
}