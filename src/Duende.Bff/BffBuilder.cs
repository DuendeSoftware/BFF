// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using Duende.Bff;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Encapsulates DI options for Duende.BFF
    /// </summary>
    public class BffBuilder
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="services"></param>
        public BffBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <summary>
        /// The service collection
        /// </summary>
        public IServiceCollection Services { get; }
        
        /// <summary>
        /// Adds a server-side session store using the in-memory store
        /// </summary>
        /// <returns></returns>
        public BffBuilder AddServerSideSessions()
        {
            Services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureApplicationCookieTicketStore>();
            Services.AddTransient<ITicketStore, ServerSideTicketStore>();

            Services.TryAddSingleton<IUserSessionStore, InMemoryUserSessionStore>();
            Services.AddTransient<ISessionRevocationService>(x => x.GetRequiredService<IUserSessionStore>());

            return this;
        }

        /// <summary>
        /// Adds a server-side session store using the supplied session store implementation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BffBuilder AddServerSideSessions<T>()
            where T : class, IUserSessionStore
        {
            AddServerSideSessions();
            Services.AddTransient<IUserSessionStore, T>();

            return this;
        }
        
        /// <summary>
        /// Adds a custom factory to create HTTP clients for remote BFF API endpoints
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public BffBuilder AddHttpMessageInvokerFactory<T>()
            where T : class, IHttpMessageInvokerFactory
        {
            Services.AddTransient<IHttpMessageInvokerFactory, T>();

            return this;
        }
        
        /// <summary>
        /// Configures the HTTP client used to do backchannel calls to the token service for token lifetime management
        /// </summary>
        /// <param name="configureClient"></param>
        /// <returns></returns>
        public IHttpClientBuilder ConfigureTokenClient(Action<HttpClient> configureClient = null)
        {
            if (configureClient == null)
            {
                return Services.AddHttpClient(AccessTokenManagementDefaults.BackChannelHttpClientName);
            }

            return Services.AddHttpClient(AccessTokenManagementDefaults.BackChannelHttpClientName, configureClient);
        }
    }
}