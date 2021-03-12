
using Duende.Bff;

namespace Microsoft.AspNetCore.Builder
{
    public static class EndpointConventionBuilderExtensions
    {
        public static IEndpointConventionBuilder AsBffApiEndpoints(this IEndpointConventionBuilder builder)
        {
            return builder.WithMetadata(new BffApiEndpointAttribute());
        }
    }
}