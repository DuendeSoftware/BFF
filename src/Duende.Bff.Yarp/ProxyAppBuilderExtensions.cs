// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;

namespace Duende.Bff.Yarp
{
    public static class ProxyAppBuilderExtensions
    {
        public static IApplicationBuilder UseAntiforgeryCheck(this IApplicationBuilder yarpApp)
        {
            return yarpApp.UseMiddleware<AntiforgeryMiddleware>();
        }
    }
}