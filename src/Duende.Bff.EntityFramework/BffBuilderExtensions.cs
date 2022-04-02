// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Duende.Bff.EntityFramework.Interfaces;
using Duende.Bff.EntityFramework.Options;

namespace Duende.Bff.EntityFramework
{
    /// <summary>
    /// Extensions for BffBuilder
    /// </summary>
    public static class BffBuilderExtensions
    {
        /// <summary>
        /// Adds entity framework core support for user session store.
        /// </summary>
        /// <param name="bffBuilder"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static BffBuilder AddEntityFrameworkServerSideSessions(this BffBuilder bffBuilder, Action<IServiceProvider, DbContextOptionsBuilder> action)
        {
            bffBuilder.Services.AddDbContext<SessionDbContext>(action);
            bffBuilder.Services.AddTransient<IUserSessionStoreCleanup, UserSessionStore>();
            return bffBuilder.AddServerSideSessions<UserSessionStore>();
        }
        
        /// <summary>
        /// Adds entity framework core support for user session store.
        /// </summary>
        /// <param name="bffBuilder"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static BffBuilder AddEntityFrameworkServerSideSessions(this BffBuilder bffBuilder, Action<DbContextOptionsBuilder> action)
        {
            bffBuilder.Services.AddDbContext<SessionDbContext>(action);
            bffBuilder.Services.AddScoped<ISessionDbContext, SessionDbContext>();
            bffBuilder.Services.AddTransient<IUserSessionStoreCleanup, UserSessionStore>();
            return bffBuilder.AddEntityFrameworkServerSideSessions<SessionDbContext>();
        }

        /// <summary>
        /// Adds entity framework core support for user session store.
        /// </summary>
        /// <param name="bffBuilder"></param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static BffBuilder AddEntityFrameworkServerSideSessions(this BffBuilder bffBuilder, Action<SessionStoreOptions> storeOptionsAction = null)
        {
            return bffBuilder.AddEntityFrameworkServerSideSessions<SessionDbContext>(storeOptionsAction);
        }
        
        /// <summary>
        /// Adds entity framework core support for user session store.
        /// </summary>
        /// <param name="bffBuilder"></param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static BffBuilder AddEntityFrameworkServerSideSessions<TContext>(this BffBuilder bffBuilder, Action<SessionStoreOptions> storeOptionsAction = null)
            where TContext : DbContext, ISessionDbContext
        {
            bffBuilder.Services.AddSessionDbContext<TContext>(storeOptionsAction);
            bffBuilder.Services.AddTransient<IUserSessionStoreCleanup, UserSessionStore>();
            return bffBuilder.AddServerSideSessions<UserSessionStore>();
        }
        
        /// <summary>
        /// Adds Session DbContext to the DI system.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        
        public static IServiceCollection AddSessionDbContext(this IServiceCollection services,
            Action<SessionStoreOptions> storeOptionsAction = null)
        {
            return services.AddSessionDbContext<SessionDbContext>(storeOptionsAction);
        }
        
        /// <summary>
        /// Adds Session DbContext to the DI system.
        /// </summary>
        /// <typeparam name="TContext">The ISessionDbContext to use.</typeparam>
        /// <param name="services"></param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IServiceCollection AddSessionDbContext<TContext>(this IServiceCollection services, Action<SessionStoreOptions> storeOptionsAction = null)
            where TContext : DbContext, ISessionDbContext
        {
            var options = new SessionStoreOptions();
            services.AddSingleton(options);
            storeOptionsAction?.Invoke(options);

            if (options.ResolveDbContextOptions != null)
            {
                if (options.EnablePooling)
                {
                    if (options.PoolSize.HasValue)
                    {
                        services.AddDbContextPool<TContext>(options.ResolveDbContextOptions, options.PoolSize.Value);
                    }
                    else
                    {
                        services.AddDbContextPool<TContext>(options.ResolveDbContextOptions);
                    }
                }
                else
                {
                    services.AddDbContext<TContext>(options.ResolveDbContextOptions);
                }
            }
            else
            {
                if (options.EnablePooling)
                {
                    if (options.PoolSize.HasValue)
                    {
                        services.AddDbContextPool<TContext>(
                            dbCtxBuilder => { options.ConfigureDbContext?.Invoke(dbCtxBuilder); }, options.PoolSize.Value);
                    }
                    else
                    {
                        services.AddDbContextPool<TContext>(
                            dbCtxBuilder => { options.ConfigureDbContext?.Invoke(dbCtxBuilder); });
                    }
                }
                else
                {
                    services.AddDbContext<TContext>(dbCtxBuilder =>
                    {
                        options.ConfigureDbContext?.Invoke(dbCtxBuilder);
                    });
                }
            }

            services.AddScoped<ISessionDbContext, TContext>();

            return services;
        }
    }
}
