using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Duende.Bff.Endpoints
{
    public class BffApiAntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BffApiAntiforgeryMiddleware> _logger;

        public BffApiAntiforgeryMiddleware(RequestDelegate next, ILogger<BffApiAntiforgeryMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint == null)
            {
                await _next(context);
                return;
            }
            
            var md = endpoint.Metadata.GetMetadata<BffApiEndpointAttribute>();

            if (md != null)
            {
                if (md.DisableAntiforgeryCheck == false)
                {
                    var antiForgeryHeader = context.Request.Headers["X-CSRF"].FirstOrDefault();
                    if (antiForgeryHeader == null || antiForgeryHeader != "1")
                    {
                        _logger.AntiforgeryValidationFailed(context.Request.Path);
                        
                        context.Response.StatusCode = 401;
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}