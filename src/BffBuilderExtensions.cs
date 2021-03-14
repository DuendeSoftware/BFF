using Duende.Bff.Endpoints;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the BFF middleware
    /// </summary>
    public static class BffBuilderExtensions
    {
        /// <summary>
        /// Adds the Duende.BFF middleware to the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseBff(this IApplicationBuilder app)
        {
            return app.UseMiddleware<BffMiddleware>();
        }
    }
}