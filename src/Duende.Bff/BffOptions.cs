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
        /// Base path for management endpoints. Defaults to "/bff".
        /// </summary>
        public PathString ManagementBasePath { get; set; } = "/bff";

        /// <summary>
        /// Specifies whether the user endpoint requires the antiforgery header
        /// </summary>
        public bool RequireAntiforgeryHeaderForUserEndpoint { get; set; } = true;
        
        /// <summary>
        /// Flag that specifies if the *sid* claim needs to be present in the logout request as query string parameter. 
        /// Used to prevent cross site request forgery.
        /// Defaults to true.
        /// </summary>
        public bool RequireLogoutSessionId { get; set; } = true;

        /// <summary>
        /// Specifies if the user's refresh token is automatically revoked at logout time.
        /// Defaults to true.
        /// </summary>
        public bool RevokeRefreshTokenOnLogout { get; set; } = true;

        /// <summary>
        /// Specifies if during backchannel logout all matching user sessions are logged out.
        /// If true, all sessions for the subject will be revoked. If false, just the specific 
        /// session will be revoked.
        /// Defaults to false.
        /// </summary>
        public bool BackchannelLogoutAllUserSessions { get; set; }

        /// <summary>
        /// Specifies the name of the header used for anti-forgery header protection.
        /// Defaults to X-CSRF.
        /// </summary>
        public string AntiForgeryHeaderName { get; set; } = "X-CSRF";

        /// <summary>
        /// Specifies the expected value of the anti-forgery header.
        /// Defaults to 1.
        /// </summary>
        public string AntiForgeryHeaderValue { get; set; } = "1";

        /// <summary>
        /// Specifies if X-Forwarded headers are automatically added to calls to remote API endpoints.
        /// Defaults to true.
        /// </summary>
        public bool AddXForwardedHeaders { get; set; } = true;
        
        /// <summary>
        /// Specifies if incoming XForwarded headers should be forwarded.
        /// Make sure you only forward headers from sources you trust.
        /// Defaults to false.
        /// </summary>
        public bool ForwardIncomingXForwardedHeaders { get; set; } = false;
        
        /// <summary>
        /// Specifies additional headers to forward to remote API endpoints.
        /// </summary>
        public ISet<string> ForwardedHeaders { get; set; } = new HashSet<string>();

        /// <summary>
        /// Specifies how the path for remote API endpoints gets transformed.
        /// Defaults to removing the configured local prefix.
        /// </summary>
        public PathStringTransform.PathTransformMode PathTransformMode { get; set; } =
            PathStringTransform.PathTransformMode.RemovePrefix;

        /// <summary>
        /// Specifies if the management endpoints check that the BFF middleware is added to the pipeline.
        /// </summary>
        public bool EnforceBffMiddlewareOnManagementEndpoints { get; set; } = true;

        /// <summary>
        /// License key
        /// </summary>
        public string LicenseKey { get; set; }
        
        /// <summary>
        /// Login endpoint
        /// </summary>
        public PathString LoginPath => ManagementBasePath.Add(Constants.ManagementEndpoints.Login);
        /// <summary>
        /// Logout endpoint
        /// </summary>
        public PathString LogoutPath => ManagementBasePath.Add(Constants.ManagementEndpoints.Logout);
        /// <summary>
        /// User endpoint
        /// </summary>
        public PathString UserPath => ManagementBasePath.Add(Constants.ManagementEndpoints.User);
        /// <summary>
        /// Back channel logout endpoint
        /// </summary>
        public PathString BackChannelLogoutPath => ManagementBasePath.Add(Constants.ManagementEndpoints.BackChannelLogout);
    }
}