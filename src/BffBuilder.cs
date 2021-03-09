using System;
using System.Net.Http;
using Duende.Bff;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public class BffBuilder
    {
        public BffBuilder(IServiceCollection services)
        {
            Services = services;
        }
        
        public IServiceCollection Services { get; set; }
        
        public BffBuilder AddCookieTicketStore()
        {
            Services.AddTransient<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureBffApplicationCookie>();
            Services.AddTransient<ITicketStore, CookieTicketStore>();

            Services.TryAddSingleton<IUserSessionStore, InMemoryTicketStore>();
            Services.AddTransient<ISessionRevocationService>(x => x.GetRequiredService<IUserSessionStore>());

            return this;
        }

        public BffBuilder AddCookieTicketStore<T>()
            where T : class, IUserSessionStore
        {
            AddCookieTicketStore();
            Services.AddTransient<IUserSessionStore, T>();

            return this;
        }
        
        public BffBuilder AddHttpMessageInvokerFactory<T>()
            where T : class, IHttpMessageInvokerFactory
        {
            Services.AddTransient<IHttpMessageInvokerFactory, T>();

            return this;
        }
        
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