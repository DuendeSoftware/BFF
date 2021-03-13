using Duende.Bff.Endpoints;

namespace Microsoft.AspNetCore.Builder
{
    public static class BffBuilderExtensions
    {
        public static IApplicationBuilder UseBff(this IApplicationBuilder app)
        {
            return app.UseMiddleware<BffMiddleware>();
        }
    }
}