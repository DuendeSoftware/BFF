// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Service.RuntimeModel.Transforms;

namespace Duende.Bff
{
    /// <summary>
    /// Options for BFF
    /// </summary>
    public class BffOptions
    {
        /// <summary>
        /// Flag that requires sid claim to be present in the logout request. 
        /// Used to prevent cross site request forgery.
        /// Defaults to true.
        /// </summary>
        public bool RequireLogoutSessionId { get; set; } = true;

        /// <summary>
        /// Base path for management endpoints
        /// </summary>
        public PathString ManagementBasePath = "/bff";

        /// <summary>
        /// Anti-forgery header name
        /// </summary>
        public string AntiForgeryHeaderName = "X-CSRF";

        /// <summary>
        /// Anti-forgery header value
        /// </summary>
        public string AntiForgeryHeaderValue = "1";

        /// <summary>
        /// Additional headers to forward to remote API endpoints
        /// </summary>
        public ISet<string> ForwardedHeaders = new HashSet<string>();

        /// <summary>
        /// Specifies if X-Forwarded headers are automatically added to call to remote API endpoints.
        /// Defaults to true.
        /// </summary>
        public bool AddXForwardedHeaders { get; set; } = true;

        // TODO: security notice?
        /// <summary>
        /// Forward incoming XForwarded headers.
        /// Defaults to false.
        /// </summary>
        public bool ForwardIncomingXForwardedHeaders { get; set; } = false;

        /// <summary>
        /// Specifies how the path for remote API endpoints gets transformed
        /// </summary>
        public PathStringTransform.PathTransformMode PathTransformMode { get; set; } =
            PathStringTransform.PathTransformMode.RemovePrefix;
    }
}