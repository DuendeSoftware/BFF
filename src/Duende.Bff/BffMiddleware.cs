// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Duende.Bff.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Duende.Bff.Endpoints;

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
        // add marker so we can determine if middleware has run later in the pipeline
        context.Items[Constants.BffMiddlewareMarker] = true;

        // inbound: add CSRF check for local APIs 

        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            await _next(context);
            return;
        }

        var localEndpointMetadata = endpoint.Metadata.GetOrderedMetadata<BffApiAttribute>();
        if (localEndpointMetadata.Any())
        {
            var requireLocalAntiForgeryCheck = localEndpointMetadata.First().RequireAntiForgeryCheck;
            if (requireLocalAntiForgeryCheck)
            {
                if (!context.CheckAntiForgeryHeader(_options))
                {
                    _logger.AntiForgeryValidationFailed(context.Request.Path);

                    context.Response.StatusCode = 401;
                    return;
                }
            }
        }
        else
        {
            var remoteEndpoint = endpoint.Metadata.GetMetadata<BffRemoteApiEndpointMetadata>();
            if (remoteEndpoint is { RequireAntiForgeryHeader: true })
            {
                if (!context.CheckAntiForgeryHeader(_options))
                {
                    _logger.AntiForgeryValidationFailed(context.Request.Path);

                    context.Response.StatusCode = 401;
                    return;
                }
            }
        }

        await _next(context);
    }
}