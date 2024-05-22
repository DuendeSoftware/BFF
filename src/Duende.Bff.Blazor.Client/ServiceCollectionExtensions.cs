using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Duende.Bff.Blazor.Wasm;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBff(this IServiceCollection services)
    {
        return services
            .AddAuthorizationCore()
            .AddCascadingAuthenticationState()
            .AddBffAuthenticationStateProvider();
    }

    public static IServiceCollection AddBffAuthenticationStateProvider(this IServiceCollection services)
    {
        services.AddScoped<AuthenticationStateProvider, BffAuthenticationStateProvider>();

        services.AddSingleton<AntiforgeryHandler>();

        services.AddRemoteApiHttpClient<BffAuthenticationStateProvider>((sp, client) =>
        {
            // TODO - Is this hosting model always used? I think so...
            var host = sp.GetRequiredService<IWebAssemblyHostEnvironment>();
            client.BaseAddress = new Uri(host.BaseAddress);
        });

        return services;
    }

    public static IHttpClientBuilder AddRemoteApiHttpClient(this IServiceCollection services, string clientName, Action<IServiceProvider, HttpClient> configureClient)
    {
        return services.AddHttpClient(clientName, configureClient)
             .AddHttpMessageHandler<AntiforgeryHandler>();
    }

    public static IHttpClientBuilder AddRemoteApiHttpClient(this IServiceCollection services, string clientName, Action<HttpClient> configureClient)
    {
       return services.AddHttpClient(clientName, configureClient)
            .AddHttpMessageHandler<AntiforgeryHandler>();
    }

    public static IHttpClientBuilder AddRemoteApiHttpClient<T>(this IServiceCollection services, Action<IServiceProvider, HttpClient> configureClient)
        where T : class
    {
        return services.AddHttpClient<T>(configureClient)
             .AddHttpMessageHandler<AntiforgeryHandler>();
    }

    public static IHttpClientBuilder AddRemoteApiHttpClient<T>(this IServiceCollection services, Action<HttpClient> configureClient)
        where T : class
    {
        return services.AddHttpClient<T>(configureClient)
            .AddHttpMessageHandler<AntiforgeryHandler>();
    }
}