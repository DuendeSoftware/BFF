// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Duende.Bff
{
    /// <summary>
    /// Converts Challenge/Forbid to Ajax friendly status codes for BFF API endpoints
    /// </summary>
    public class BffAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _handler;

        /// <summary>
        /// ctor
        /// </summary>
        public BffAuthorizationMiddlewareResultHandler()
        {
            _handler = new AuthorizationMiddlewareResultHandler();
        }
        
        /// <inheritdoc />
        public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            
            var remoteApi = endpoint.Metadata.GetMetadata<BffRemoteApiEndpointMetadata>();
            var localApi = endpoint.Metadata.GetMetadata<BffApiAttribute>();

            if (remoteApi == null && localApi == null)
            {
                return _handler.HandleAsync(next, context, policy, authorizeResult);
            }
            
            if (authorizeResult.Challenged)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            }

            if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return Task.CompletedTask;
            }

            return _handler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}