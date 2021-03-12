#if NET5_0

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Duende.Bff
{
    public class BffAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _handler;

        public BffAuthorizationMiddlewareResultHandler()
        {
            _handler = new AuthorizationMiddlewareResultHandler();
        }
        
        public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            var endpoint = context.GetEndpoint();
            var md = endpoint.Metadata.GetMetadata<BffApiAccessTokenMetadata>();
            var bffAttribute = endpoint.Metadata.GetMetadata<BffApiEndpointAttribute>();

            if (md != null || bffAttribute != null)
            {
                if (authorizeResult.Challenged)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Task.CompletedTask;
                }
                else if (authorizeResult.Forbidden)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return Task.CompletedTask;
                }
            }

            return _handler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
#endif