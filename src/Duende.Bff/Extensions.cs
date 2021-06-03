// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Duende.Bff
{
    public static class Extensions
    {
        public static void CheckForBffMiddleware(this HttpContext context, BffOptions options)
        {
            if (options.EnforceBffMiddlewareOnManagementEndpoints)
            {
                var found = context.Items.TryGetValue(Constants.BffMiddlewareMarker, out _);
                if (!found)
                {
                    throw new InvalidOperationException(
                        "The BFF middleware is missing in the pipeline. Add 'app.UseBff' after 'app.UseRouting' but before 'app.UseAuthorization'");
                }
            }
        }

        public static bool CheckAntiForgeryHeader(this HttpContext context, BffOptions options)
        {
            var antiForgeryHeader = context.Request.Headers[options.AntiForgeryHeaderName].FirstOrDefault();
            return antiForgeryHeader != null && antiForgeryHeader == options.AntiForgeryHeaderValue;
        }
    }
}