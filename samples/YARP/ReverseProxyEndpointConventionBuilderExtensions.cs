// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using Duende.Bff;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace YarpHost
{
    public static class ReverseProxyEndpointConventionBuilderExtensions
    {
        public static ReverseProxyConventionBuilder MapBffReverseProxy(this IEndpointRouteBuilder endpoints,
            Action<IReverseProxyApplicationBuilder> configureAction)
        {
            return endpoints.MapReverseProxy(configureAction)
                .AsBffApiEndpoint();
        }
        
        public static ReverseProxyConventionBuilder MapBffReverseProxy(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapReverseProxy()
                .AsBffApiEndpoint();
        }
        
        public static ReverseProxyConventionBuilder AsBffApiEndpoint(this ReverseProxyConventionBuilder builder,
            bool requireAntiforgeryCheck = true)
        {
            return builder.WithMetadata(new BffApiAttribute(requireAntiforgeryCheck));
        }
    }
}