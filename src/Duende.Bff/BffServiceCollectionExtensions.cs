// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

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
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static BffBuilder AddBff(this IServiceCollection services, Action<BffOptions> configureAction = null)
        {
            var opts = new BffOptions();
            configureAction?.Invoke(opts);
            services.AddSingleton(opts);

            services.AddHttpProxy();
            services.AddAccessTokenManagement();

            services.TryAddSingleton<IHttpMessageInvokerFactory, DefaultHttpMessageInvokerFactory>();
            services.AddTransient<ILoginService, DefaultLoginService>();
            services.AddTransient<ILogoutService, DefaultLogoutService>();
            services.AddTransient<IUserService, DefaultUserService>();
            services.AddTransient<IBackchannelLogoutService, DefaultBackchannelLogoutService>();
            services.TryAddTransient<ISessionRevocationService, NopSessionRevocationService>();
            
            #if NET5_0
            services.AddTransient<IAuthorizationMiddlewareResultHandler, BffAuthorizationMiddlewareResultHandler>();
            #endif
            
            return new BffBuilder(services);
        }
    }
}