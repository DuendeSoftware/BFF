using Microsoft.AspNetCore.Builder;

namespace Duende.Bff
{
    public class BffApiEndoint
    {
        public bool RequireAntiForgeryToken { get; set; }

        public AccessTokenRequirement? AccessTokenRequirement;
    }
}