// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace Duende.Bff
{
    /// <summary>
    /// Various default values
    /// </summary>
    public static class BffDefaults
    {
        /// <summary>
        /// Base path for management endpoints
        /// </summary>
        public const string ManagementBasePath = "/bff";
        
        /// <summary>
        /// Anti-forgery header name
        /// </summary>
        public const string AntiForgeryHeaderName = "X-CSRF";
        
        /// <summary>
        /// Anti-forgery header value
        /// </summary>
        public const string AntiForgeryHeaderValue = "1";
    }
}