// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Duende.Bff.Endpoints
{
    /// <summary>
    /// Middleware to provide anti-forgery protection via a static header and 302 to 401 conversion
    /// Must run *before* the authorization middleware
    /// </summary>
    public class BffMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BffOptions _options;
        private readonly ILogger<BffMiddleware> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public BffMiddleware(RequestDelegate next, BffOptions options, ILogger<BffMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }
        
        /// <summary>
        /// Request processing
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            // inbound: add CSRF check for local APIs 
            
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }
            
            var localEndoint = endpoint.Metadata.GetMetadata<BffLocalApiEndpointAttribute>();

            if (localEndoint != null)
            {
                if (localEndoint.DisableAntiForgeryCheck == false)
                {
                    var antiForgeryHeader = context.Request.Headers[_options.AntiForgeryHeaderName].FirstOrDefault();
                    if (antiForgeryHeader == null || antiForgeryHeader != _options.AntiForgeryHeaderValue)
                    {
                        _logger.AntiForgeryValidationFailed(context.Request.Path);
                        
                        context.Response.StatusCode = 401;
                        return;
                    }
                }
            }

            await _next(context);
            
            // outbound: for .NET Core 3.1 - we assume that an API will never return a 302
            // if a 302 is returned, that must be the challenge to the OIDC provider
            // we convert this to a 401
            // in .NET 5 we can use the AuthorizationMiddlewareResultHandler for this logic
            
#if NETCOREAPP3_1
            var remoteEndoint = endpoint.Metadata.GetMetadata<BffRemoteApiEndpointMetadata>();

            if (localEndoint != null || remoteEndoint != null)
            {
                if (context.Response.StatusCode == 302)
                {
                    context.Response.StatusCode = 401;
                    context.Response.Headers["Location"] = "";
                }
            }
#endif
        }
    }
}