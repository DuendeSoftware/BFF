// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Duende.Bff.Yarp
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
        /// The YARP transform builder
        /// </summary>
        protected readonly ITransformBuilder TransformBuilder;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="options">The BFF options</param>
        /// <param name="transformBuilder">The YARP transform builder</param>
        public DefaultHttpTransformerFactory(BffOptions options, ITransformBuilder transformBuilder)
        {
            Options = options;
            TransformBuilder = transformBuilder;
        }
        
        /// <inheritdoc />
        public virtual HttpTransformer CreateTransformer(string localPath, string accessToken = null)
        {
            return TransformBuilder.Create(context =>
            {
                // apply default YARP logic for forwarding headers
                context.CopyRequestHeaders = true;
                
                // use YARP default logic for x-forwarded headers
                context.UseDefaultForwarders = true;
                
                // always remove cookie header since this contains the session
                context.RequestTransforms.Add(new RequestHeaderRemoveTransform("Cookie"));
                
                // transform path to remove prefix
                context.RequestTransforms.Add(new PathStringTransform(PathStringTransform.PathTransformMode.RemovePrefix, localPath));
                
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    context.RequestTransforms.Add(new AccessTokenRequestTransform(accessToken));
                }
            });
        }
    }
}