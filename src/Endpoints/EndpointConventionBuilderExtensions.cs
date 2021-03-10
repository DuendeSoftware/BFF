
using Duende.Bff;

namespace Microsoft.AspNetCore.Builder
{
    public static class EndpointConventionBuilderExtensions
    {
        public static IEndpointConventionBuilder AsBffApiEndpoint(this IEndpointConventionBuilder builder)
        {
            return builder.WithMetadata(new BffApiEndointMetadata());
        }
    }
}