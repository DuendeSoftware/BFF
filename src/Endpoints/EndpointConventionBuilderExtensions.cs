
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
        /// <returns></returns>
        public static IEndpointConventionBuilder AsLocalBffApiEndpoint(this IEndpointConventionBuilder builder)
        {
            return builder.WithMetadata(new BffLocalApiEndpointAttribute());
        }
    }
}