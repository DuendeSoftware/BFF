// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

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
        /// Specifies if the management endpoints check that the BFF middleware is added to the pipeline.
        /// </summary>
        public bool EnforceBffMiddleware { get; set; } = true;

        /// <summary>
        /// License key
        /// </summary>
        public string? LicenseKey { get; set; }
        
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
        
        /// <summary>
        /// Diagnostics endpoint
        /// </summary>
        public PathString DiagnosticsPath => ManagementBasePath.Add(Constants.ManagementEndpoints.Diagnostics);
    }
}