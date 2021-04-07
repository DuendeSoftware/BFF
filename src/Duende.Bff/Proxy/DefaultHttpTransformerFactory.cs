using System.Collections.Generic;
using System.Linq;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Service.Proxy;
using Yarp.ReverseProxy.Service.RuntimeModel.Transforms;

namespace Duende.Bff
{
    /// <summary>
    /// Default HTTP transformer implementation
    /// </summary>
    public class DefaultHttpTransformerFactory : IHttpTransformerFactory
    {
        /// <summary>
        /// The options
        /// </summary>
        protected readonly BffOptions Options;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options"></param>
        public DefaultHttpTransformerFactory(BffOptions options)
        {
            Options = options;
        }
        
        private static readonly string ForKey = "For";
        private static readonly string HostKey = "Host";
        private static readonly string ProtoKey = "Proto";
        private static readonly string PathBaseKey = "PathBase";
        
        /// <inheritdoc />
        public virtual HttpTransformer CreateClient(string localPath, string accessToken = null)
        {
            var requestTransforms = new List<RequestTransform>
            {
                new PathStringTransform(Options.PathTransformMode, localPath),
                new ForwardHeadersTransform(new[] { HeaderNames.Accept, HeaderNames.ContentLength, HeaderNames.ContentType })
            };

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                requestTransforms.Add(new AccessTokenTransform(accessToken));
            }

            if (Options.ForwardedHeaders.Any())
            {
                requestTransforms.Add(new ForwardHeadersTransform(Options.ForwardedHeaders));
            }

            if (Options.AddXForwardedHeaders)
            {
                string headerPrefix = "X-Forwarded-";

                requestTransforms.Add(
                    new RequestHeaderXForwardedForTransform(headerPrefix + ForKey, true));
                requestTransforms.Add(
                    new RequestHeaderXForwardedHostTransform(headerPrefix + HostKey, true));
                requestTransforms.Add(
                    new RequestHeaderXForwardedProtoTransform(headerPrefix + ProtoKey, true));
                requestTransforms.Add(
                    new RequestHeaderXForwardedPathBaseTransform(headerPrefix + PathBaseKey, true));
            }
            
            var transformer = new StructuredTransformer(
                copyRequestHeaders: false,
                copyResponseHeaders: true,
                copyResponseTrailers: true,
                requestTransforms,
                responseTransforms: new List<ResponseTransform>(),
                responseTrailerTransforms: new List<ResponseTrailersTransform>());

            return transformer;
        }
    }
}