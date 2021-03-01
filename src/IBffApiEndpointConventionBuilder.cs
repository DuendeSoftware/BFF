using System.Linq;
using Duende.Bff;

namespace Microsoft.AspNetCore.Builder
{
    public static class Foo
    {
        public static TBuilder RequireAntiforgeryToken<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                var options =
                    endpointBuilder.Metadata.First(m => m.GetType() == typeof(BffApiEndointMetadata)) as BffApiEndointMetadata;
                
                options.RequireAntiForgeryToken = true;
            });

            return builder;
        }
        
        public static TBuilder RequireAccessToken<TBuilder>(this TBuilder builder, TokenType type = TokenType.User) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                var options =
                    endpointBuilder.Metadata.First(m => m.GetType() == typeof(BffApiEndointMetadata)) as BffApiEndointMetadata;

                if (type == TokenType.User)   options.AccessTokenRequirement = AccessTokenRequirement.RequireUserToken;
                if (type == TokenType.Client) options.AccessTokenRequirement = AccessTokenRequirement.RequireClientToken;
            });

            return builder;
        }
    }

    public enum TokenType
    {
        User, 
        Client
    }
}