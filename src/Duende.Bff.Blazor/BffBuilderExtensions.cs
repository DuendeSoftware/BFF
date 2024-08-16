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
        builder.Services.AddScoped<AuthenticationStateProvider, BffServerAuthenticationStateProvider>();
        builder.Services.AddScoped<CaptureManagementClaimsCookieEvents>();

        return builder;
    }
}