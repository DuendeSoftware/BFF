using Duende.Bff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

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

            return services;
        }
    }
}