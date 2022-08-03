// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Microsoft.AspNetCore.Builder;

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
    public static BffBuilder AddBff(this IServiceCollection services, Action<BffOptions>? configureAction = null)
    {
        if (configureAction != null)
        {
            services.Configure(configureAction);
        }

        services.AddDistributedMemoryCache();
        services.AddOpenIdConnectAccessTokenManagement();

        // management endpoints
        services.AddTransient<ILoginService, DefaultLoginService>();
        services.AddTransient<ISilentLoginService, DefaultSilentLoginService>();
        services.AddTransient<ISilentLoginCallbackService, DefaultSilentLoginCallbackService>();
        services.AddTransient<ILogoutService, DefaultLogoutService>();
        services.AddTransient<IUserService, DefaultUserService>();
        services.AddTransient<IBackchannelLogoutService, DefaultBackchannelLogoutService>();
        services.AddTransient<IDiagnosticsService, DefaultDiagnosticsService>();

        // session management
        services.TryAddTransient<ISessionRevocationService, NopSessionRevocationService>();

        // cookie configuration
        services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureSlidingExpirationCheck>();
        services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureApplicationCookieRevokeRefreshToken>();
        
        services.AddSingleton<IPostConfigureOptions<OpenIdConnectOptions>, PostConfigureOidcOptionsForSilentLogin>();

        services.AddTransient<IAuthorizationMiddlewareResultHandler, BffAuthorizationMiddlewareResultHandler>();

        return new BffBuilder(services);
    }
}