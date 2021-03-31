// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

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
        public string ManagementBasePath = "/bff";

        /// <summary>
        /// Anti-forgery header name
        /// </summary>
        public string AntiForgeryHeaderName = "X-CSRF";

        /// <summary>
        /// Anti-forgery header value
        /// </summary>
        public string AntiForgeryHeaderValue = "1";
    }
}