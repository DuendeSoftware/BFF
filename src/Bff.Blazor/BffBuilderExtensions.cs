// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Duende.Bff.Blazor;

public static class BffBuilderExtensions
{
    public static BffBuilder AddBlazorServer(this BffBuilder builder)
    {
        builder.Services.AddOpenIdConnectAccessTokenManagement()
            .AddBlazorServerAccessTokenManagement<ServerSideTokenStore>();

        var removeThis = builder.Services.First(d => d.ImplementationType == typeof(ServerSideTokenStore));
        builder.Services.Remove(removeThis);
        builder.Services.AddScoped<IUserTokenStore, ServerSideTokenStore>();

        builder.Services.AddScoped<AuthenticationStateProvider, BffServerAuthenticationStateProvider>();
        builder.Services.AddScoped<CaptureManagementClaimsCookieEvents>();

        return builder;
    }
}