// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Duende.Bff.Blazor.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBff(this IServiceCollection services,
        Action<BffBlazorOptions>? configureAction = null)
    {
        if (configureAction != null)
        {
            services.Configure(configureAction);
        }

        services
            .AddAuthorizationCore()
            .AddScoped<AuthenticationStateProvider, BffClientAuthenticationStateProvider>()
            .AddCascadingAuthenticationState()
            .AddTransient<AntiforgeryHandler>()
            .AddHttpClient("BffAuthenticationStateProvider", (sp, client) =>
            {
                var baseAddress = GetBaseAddress(sp);
                client.BaseAddress = new Uri(baseAddress);
            }).AddHttpMessageHandler<AntiforgeryHandler>();

        return services;
    }

    private static string GetBaseAddress(IServiceProvider sp)
    {
        var opt = sp.GetRequiredService<IOptions<BffBlazorOptions>>();
        if (opt.Value.RemoteApiBaseAddress != null)
        {
            return opt.Value.RemoteApiBaseAddress;
        }
        else
        {
            var hostEnv = sp.GetRequiredService<IWebAssemblyHostEnvironment>();
            return hostEnv.BaseAddress;
        }
    }

    private static string GetRemoteApiPath(IServiceProvider sp)
    {
        var opt = sp.GetRequiredService<IOptions<BffBlazorOptions>>();
        return opt.Value.RemoteApiPath;
    }

    private static Action<IServiceProvider, HttpClient> SetBaseAddressInConfigureClient(
        Action<IServiceProvider, HttpClient>? configureClient)
    {
        return (sp, client) =>
        {
            SetBaseAddress(sp, client);
            configureClient?.Invoke(sp, client);
        };
    }

    private static Action<IServiceProvider, HttpClient> SetBaseAddressInConfigureClient(
        Action<HttpClient>? configureClient)
    {
        return (sp, client) =>
        {
            SetBaseAddress(sp, client);
            configureClient?.Invoke(client);
        };
    }

    private static void SetBaseAddress(IServiceProvider sp, HttpClient client)
    {
        var baseAddress = GetBaseAddress(sp);
        if (!baseAddress.EndsWith("/"))
        {
            baseAddress += "/";
        }

        var remoteApiPath = GetRemoteApiPath(sp);
        if (!string.IsNullOrEmpty(remoteApiPath))
        {
            if (remoteApiPath.StartsWith("/"))
            {
                remoteApiPath = remoteApiPath.Substring(1);
            }

            if (!remoteApiPath.EndsWith("/"))
            {
                remoteApiPath += "/";
            }
        }

        client.BaseAddress = new Uri(new Uri(baseAddress), remoteApiPath);
    }

    public static IHttpClientBuilder AddRemoteApiHttpClient(this IServiceCollection services, string clientName,
        Action<HttpClient> configureClient)
    {
        return services.AddHttpClient(clientName, SetBaseAddressInConfigureClient(configureClient))
            .AddHttpMessageHandler<AntiforgeryHandler>();
    }

    public static IHttpClientBuilder AddRemoteApiHttpClient(this IServiceCollection services, string clientName,
        Action<IServiceProvider, HttpClient>? configureClient = null)
    {
        return services.AddHttpClient(clientName, SetBaseAddressInConfigureClient(configureClient))
            .AddHttpMessageHandler<AntiforgeryHandler>();
    }

    public static IHttpClientBuilder AddRemoteApiHttpClient<T>(this IServiceCollection services,
        Action<HttpClient> configureClient)
        where T : class
    {
        return services.AddHttpClient<T>(SetBaseAddressInConfigureClient(configureClient))
            .AddHttpMessageHandler<AntiforgeryHandler>();
    }

    public static IHttpClientBuilder AddRemoteApiHttpClient<T>(this IServiceCollection services,
        Action<IServiceProvider, HttpClient>? configureClient = null)
        where T : class
    {
        return services.AddHttpClient<T>(SetBaseAddressInConfigureClient(configureClient))
            .AddHttpMessageHandler<AntiforgeryHandler>();
    }
}