// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

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
        public virtual HttpTransformer CreateTransformer(string localPath, string accessToken = null)
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
                var append = Options.ForwardIncomingXForwardedHeaders;

                requestTransforms.Add(
                    new RequestHeaderXForwardedForTransform(headerPrefix + ForKey, append));
                requestTransforms.Add(
                    new RequestHeaderXForwardedHostTransform(headerPrefix + HostKey, append));
                requestTransforms.Add(
                    new RequestHeaderXForwardedProtoTransform(headerPrefix + ProtoKey, append));
                requestTransforms.Add(
                    new RequestHeaderXForwardedPathBaseTransform(headerPrefix + PathBaseKey, append));
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