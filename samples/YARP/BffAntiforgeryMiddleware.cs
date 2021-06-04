// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Duende.Bff;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Model;

namespace YARP.Sample
{
    public class BffAntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BffOptions _options;
        private readonly ILogger _logger;

        public BffAntiforgeryMiddleware(RequestDelegate next, BffOptions options, ILogger<BffAntiforgeryMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.CheckAntiForgeryHeader(_options))
            {
                var feature = context.Features.Get<IReverseProxyFeature>();
                _logger.LogError("Antiforgery validation failed for {path}", feature.Route.Config.RouteId);
                
                //_logger.AntiForgeryValidationFailed(context.Request.Path);
                
                context.Response.StatusCode = 401;
                return;
            }

            await _next(context);
        }
    }
}