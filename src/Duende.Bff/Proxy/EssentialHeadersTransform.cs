using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Service.RuntimeModel.Transforms;

namespace Duende.Bff
{
    /// <summary>
    /// Adds an access token to outgoing requests
    /// </summary>
    public class EssentialHeadersTransform : RequestTransform
    {
        /// <inheritdoc />
        public override ValueTask ApplyAsync(RequestTransformContext context)
        {
            foreach (var header in context.HttpContext.Request.Headers)
            {
                var headerName = header.Key;
                var headerValue = header.Value;
                
                if (StringValues.IsNullOrEmpty(headerValue))
                {
                    continue;
                }

                // Filter out HTTP/2 pseudo headers like ":method" and ":path", those go into other fields.
                if (headerName.Length > 0 && headerName[0] == ':')
                {
                    continue;
                }

                if (headerName == HeaderNames.ContentLength ||
                    headerName == HeaderNames.ContentType ||
                    headerName == HeaderNames.Accept)
                {
                    RequestUtilities.AddHeader(context.ProxyRequest, headerName, headerValue);    
                }
                
            }

            return default;
        }
    }
}