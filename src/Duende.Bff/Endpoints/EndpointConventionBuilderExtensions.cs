// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for BFF endpoint conventions
    /// </summary>
    public static class EndpointConventionBuilderExtensions
    {
        /// <summary>
        /// Marks an endpoint as a local BFF API endpoint.
        /// This metadata is used by the BFF middleware.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="requireAntiforgerCheck">Specifies if the antiforgery header gets checked</param>
        /// <returns></returns>
        public static IEndpointConventionBuilder AsBffApiEndpoint(this IEndpointConventionBuilder builder, bool requireAntiforgeryCheck = true)
        {
            return builder.WithMetadata(new BffApiAttribute(requireAntiforgeryCheck));
        }
    }
}