using System.Linq;
using Duende.Bff;

namespace Microsoft.AspNetCore.Builder
{
    public static class BffApiEndpointExtensions
    {
        public static TBuilder RequireAntiforgeryToken<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                var metadata =
                    endpointBuilder.Metadata.First(m => m.GetType() == typeof(BffApiEndointMetadata)) as BffApiEndointMetadata;
                
                metadata.RequireAntiForgeryToken = true;
            });

            return builder;
        }
        
        public static TBuilder RequireAccessToken<TBuilder>(this TBuilder builder, TokenType type = TokenType.User) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                var metadata =
                    endpointBuilder.Metadata.First(m => m.GetType() == typeof(BffApiEndointMetadata)) as BffApiEndointMetadata;

                metadata.RequiredTokenType = type;
            });

            return builder;
        }
        
        public static TBuilder WithOptionalUserAccessToken<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                var metadata =
                    endpointBuilder.Metadata.First(m => m.GetType() == typeof(BffApiEndointMetadata)) as BffApiEndointMetadata;

                metadata.OptionalUserToken = true;
            });

            return builder;
        }
    }
}