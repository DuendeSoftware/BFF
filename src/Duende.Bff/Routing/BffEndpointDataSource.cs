// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Duende.Bff;

internal class BffEndpointDataSource : EndpointDataSource, IEndpointConventionBuilder
{
    private readonly List<Action<EndpointBuilder>> _conventions = new List<Action<EndpointBuilder>>();
    private readonly List<(PathString, RequestDelegate)> _endpointMap = new List<(PathString, RequestDelegate)>();

    public PathString BasePath { get; internal set; }

    private List<Endpoint> _endpoints = default!;

    private static Task ProcessWith<T>(HttpContext context)
        where T : IBffEndpointService
    {
        var service = context.RequestServices.GetRequiredService<T>();
        return service.ProcessRequestAsync(context);
    }

    internal void Map<T>(PathString path)
        where T : IBffEndpointService
    {
        _endpointMap.Add(new(path, ProcessWith<T>));
    }

    public override IReadOnlyList<Endpoint> Endpoints 
    { 
        get 
        { 
            if (_endpoints == null)
            {
                _endpoints = BuildEndpoints();
            }

            return _endpoints;
        } 
    }

    private List<Endpoint> BuildEndpoints()
    {
        var list = new List<Endpoint>();
        return list;
    }

    public override IChangeToken GetChangeToken()
    {
        return NullChangeToken.Singleton;
    }

    public void Add(Action<EndpointBuilder> convention)
    {
        _conventions.Add(convention);
    }
}
