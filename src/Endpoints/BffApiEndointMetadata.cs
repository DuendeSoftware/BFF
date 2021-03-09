namespace Duende.Bff
{
    public class BffApiEndointMetadata
    {
        public TokenType? RequiredTokenType;
        
        public bool RequireAntiForgeryToken { get; set; }
        public bool OptionalUserToken { get; set; }
    }
}