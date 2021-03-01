using Microsoft.AspNetCore.Builder;

namespace Duende.Bff
{
    public class BffApiEndointMetadata
    {
        public bool RequireAntiForgeryToken { get; set; }

        public AccessTokenRequirement? AccessTokenRequirement;
    }
}