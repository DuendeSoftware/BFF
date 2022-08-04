// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Duende.Bff;

/// <summary>
/// Endpoint helpers for BFF.
/// </summary>
public static class BffEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Adds the BffEndpointDataSource to the routing table.
    /// </summary>
    /// <param name="endpoints"></param>
    /// <param name="basePath"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IEndpointConventionBuilder MapBff(this IEndpointRouteBuilder endpoints, PathString basePath, Action<BffConfigurationBuilder> configure)
    {
        if (basePath == null)
        {
            throw new ArgumentNullException(nameof(basePath));
        }
        if (endpoints == null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        endpoints.CheckLicense();

        var dataSource = new BffEndpointDataSource(); // endpoints.ServiceProvider.GetRequiredService<BffEndpointDataSource>();

        var configurationBuilder = new BffConfigurationBuilder(basePath, dataSource);
        configure(configurationBuilder);

        endpoints.DataSources.Add(dataSource);

        return dataSource;
    }

    /// <summary>
    /// Adds the BffEndpointDataSource to the routing table.
    /// </summary>
    /// <param name="endpoints"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEndpointConventionBuilder MapBff(this IEndpointRouteBuilder endpoints, Action<BffConfigurationBuilder> configure)
    {
        return endpoints.MapBff("/bff", configure);
    }

    /// <summary>
    /// Adds all the BFF management endpoints.
    /// </summary>
    /// <param name="endpoints"></param>
    public static IEndpointConventionBuilder MapBffManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapBff(bff => 
        {
            bff.AddLogin();
            bff.AddLogout();
            bff.AddSilentLogin();
        });
    }
}
