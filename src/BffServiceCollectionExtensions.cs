using Duende.Bff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.Builder
{
    public static class BffServiceCollectionExtensions
    {
        public static BffBuilder AddBff(this IServiceCollection services)
        {
            services.AddReverseProxy().LoadFromMemory();
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