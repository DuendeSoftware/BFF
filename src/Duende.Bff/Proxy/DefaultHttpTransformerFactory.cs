// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Abstractions.Config;
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
        
        /// <inheritdoc />
        public virtual HttpTransformer CreateTransformer(string localPath, string accessToken = null)
        {
            var context = new TransformBuilderContext()
            {
                CopyRequestHeaders = false
            };
         
            context.RequestTransforms.Add( new PathStringTransform(Options.PathTransformMode, localPath));
            context.RequestTransforms.Add(new ForwardHeadersTransform(new[] { HeaderNames.Accept, HeaderNames.ContentLength, HeaderNames.ContentType }));
            
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                context.RequestTransforms.Add(new AccessTokenTransform(accessToken));
            }

            if (Options.ForwardedHeaders.Any())
            {
                context.RequestTransforms.Add(new ForwardHeadersTransform(Options.ForwardedHeaders));
            }

            if (Options.AddXForwardedHeaders)
            {
                context.AddXForwarded(append: Options.ForwardIncomingXForwardedHeaders);
            }
            
            return new StructuredTransformer(
                context.CopyRequestHeaders,
                context.CopyResponseHeaders,
                context.CopyResponseTrailers,
                context.RequestTransforms,
                context.ResponseTransforms,
                context.ResponseTrailersTransforms);
        }
    }
}