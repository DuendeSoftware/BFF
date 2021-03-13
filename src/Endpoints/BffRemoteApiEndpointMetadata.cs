namespace Duende.Bff
{
    public class BffRemoteApiEndpointMetadata
    {
        public TokenType? RequiredTokenType;
        
        public bool OptionalUserToken { get; set; }
        
        public bool RequireAntiForgeryHeader { get; set; } = true;
    }
}