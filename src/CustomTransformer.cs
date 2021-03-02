using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.ReverseProxy.Service.Proxy;

namespace Duende.Bff
{
    internal class AccessTokenTransformer : HttpTransformer
    {
        private readonly string _accessToken;

        public AccessTokenTransformer(string accessToken)
        {
            _accessToken = accessToken;
        }

        public override async Task TransformRequestAsync(HttpContext httpContext,
            HttpRequestMessage proxyRequest, string destinationPrefix)
        {
            // Copy headers normally and then remove the original host.
            // Use the destination host from proxyRequest.RequestUri instead.
            await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);
            proxyRequest.Headers.Host = null;

            if (!string.IsNullOrWhiteSpace(_accessToken))
            {
                proxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
        }
    }
}