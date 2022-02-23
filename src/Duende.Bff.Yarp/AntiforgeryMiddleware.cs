// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Duende.Bff.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Duende.Bff.Yarp
{
    public class AntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BffOptions _options;
        private readonly ILogger<AntiforgeryMiddleware> _logger;

        public AntiforgeryMiddleware(RequestDelegate next, BffOptions options, ILogger<AntiforgeryMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var proxyFeature = context.GetReverseProxyFeature();
            var route = proxyFeature.Route;

            if (route.Config.Metadata != null)
            {
                if (route.Config.Metadata.TryGetValue(Constants.Yarp.AntiforgeryCheckMetadata, out var value))
                {
                    if (string.Equals(value, true.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (!context.CheckAntiForgeryHeader(_options))
                        {
                            context.Response.StatusCode = 401;
                            _logger.AntiForgeryValidationFailed(route.Config.RouteId);

                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}