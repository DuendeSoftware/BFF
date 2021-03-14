namespace Duende.Bff
{
    /// <summary>
    /// Endpoint metadata for a remote BFF API endpoint
    /// </summary>
    public class BffRemoteApiEndpointMetadata
    {
        /// <summary>
        /// Required token type (if any)
        /// </summary>
        public TokenType? RequiredTokenType;
        
        /// <summary>
        /// Optionally send a user token if present
        /// </summary>
        public bool OptionalUserToken { get; set; }
        
        /// <summary>
        /// Require presence of anti-forgery header
        /// </summary>
        public bool RequireAntiForgeryHeader { get; set; } = true;
    }
}