using Duende.Bff;
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

            return services;
        }
    }
}