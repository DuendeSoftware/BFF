// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using Duende.Bff;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Duende.Bff
{
    /// <summary>
    /// Extensions methods to wire up BFF-enhanced YARP
    /// </summary>
    public static class ReverseProxyEndpointConventionBuilderExtensions
    {
        /// <summary>
        /// Adds YARP with anti-forgery protection
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static ReverseProxyConventionBuilder MapBffReverseProxy(this IEndpointRouteBuilder endpoints,
            Action<IReverseProxyApplicationBuilder> configureAction)
        {
            return endpoints.MapReverseProxy(configureAction)
                .AsBffApiEndpoint();
        }
        
        /// <summary>
        /// Adds YARP with anti-forgery protection 
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public static ReverseProxyConventionBuilder MapBffReverseProxy(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapReverseProxy()
                .AsBffApiEndpoint();
        }
        
        /// <summary>
        /// Adds anti-forgery protection to YARP
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="requireAntiforgeryCheck"></param>
        /// <returns></returns>
        public static ReverseProxyConventionBuilder AsBffApiEndpoint(this ReverseProxyConventionBuilder builder,
            bool requireAntiforgeryCheck = true)
        {
            return builder.WithMetadata(new BffApiAttribute(requireAntiforgeryCheck));
        }
    }
}