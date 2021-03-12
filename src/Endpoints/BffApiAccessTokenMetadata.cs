namespace Duende.Bff
{
    public class BffApiAccessTokenMetadata
    {
        public TokenType? RequiredTokenType;
        
        public bool OptionalUserToken { get; set; }
    }
}