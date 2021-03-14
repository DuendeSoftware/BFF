using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.ReverseProxy.Service.Proxy;

namespace Duende.Bff
{
    internal class BffHttpTransformer : HttpTransformer
    {
        private readonly string _accessToken;
        private readonly PathString _fullPath;
        private readonly PathString _localPath;
        private readonly QueryString _query;

        public BffHttpTransformer(string accessToken, PathString fullPath, PathString localPath, QueryString query)
        {
            _accessToken = accessToken;
            _fullPath = fullPath;
            _localPath = localPath;
            _query = query;
        }

        public override async Task TransformRequestAsync(HttpContext httpContext,
            HttpRequestMessage proxyRequest, string destinationPrefix)
        {
            await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);
            proxyRequest.Headers.Host = null;

            proxyRequest.RequestUri = MakeDestinationAddress(destinationPrefix, _fullPath, _localPath, _query);

            if (!string.IsNullOrWhiteSpace(_accessToken))
            {
                proxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
        }
        
        internal static Uri MakeDestinationAddress(string destinationPrefix, PathString fullPath, PathString localPath, QueryString query)
        {
            ReadOnlySpan<char> prefixSpan = destinationPrefix;

            if (fullPath.HasValue && destinationPrefix.EndsWith('/'))
            {
                // When PathString has a value it always starts with a '/'. Avoid double slashes when concatenating.
                prefixSpan = prefixSpan[0..^1];
            }

            fullPath = fullPath.StartsWithSegments(localPath, out var remainder) ? remainder : fullPath;
            var targetAddress = string.Concat(prefixSpan, fullPath.ToUriComponent(), query.ToUriComponent());

            return new Uri(targetAddress, UriKind.Absolute);
        }
    }
}