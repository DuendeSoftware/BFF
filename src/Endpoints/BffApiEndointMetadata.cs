namespace Duende.Bff
{
    public class BffApiEndointMetadata
    {
        public TokenType? RequiredTokenType;
        
        public bool OptionalUserToken { get; set; }

        public bool RequireAntiForgeryHeader { get; set; } = true;
    }
}