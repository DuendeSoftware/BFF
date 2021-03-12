namespace Duende.Bff
{
    public class BffApiAccessTokenMetadata
    {
        public TokenType? RequiredTokenType;
        
        public bool OptionalUserToken { get; set; }
    }
    
    public class BffApiAntiforgeryMetadata
    {
        public bool RequireAntiForgeryHeader { get; set; } = true;
    }
}