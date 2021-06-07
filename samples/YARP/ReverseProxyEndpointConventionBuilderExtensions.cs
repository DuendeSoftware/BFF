// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using Duende.Bff;
using Microsoft.AspNetCore.Builder;

namespace YarpHost
{
    public static class ReverseProxyEndpointConventionBuilderExtensions
    {
        public static ReverseProxyConventionBuilder AsBffApiEndpoint(this ReverseProxyConventionBuilder builder,
            bool requireAntiforgeryCheck = true)
        {
            return builder.WithMetadata(new BffApiAttribute(requireAntiforgeryCheck));
        }
    }
}