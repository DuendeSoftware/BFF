namespace Duende.Bff
{
    /// <summary>
    /// Constants for Duende.BFF
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Custom claim types used by Duende.BFF
        /// </summary>
        public static class ClaimTypes
        {
            /// <summary>
            /// Claim type for logout URL including session id
            /// </summary>
            public const string LogoutUrl = "bff:logout_url";
            
            /// <summary>
            /// Claim type for session expiration in seconds
            /// </summary>
            public const string SessionExpiresIn = "bff:session_expires_in";
        }
    }
}